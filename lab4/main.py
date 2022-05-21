import os
import wave
from os import path
from pydub import AudioSegment
from google.cloud import storage
from google.cloud import speech
import sounddevice as sd
from scipy.io.wavfile import write
import numpy as np

filepath = 'source/'
bucketname = 'iad_lab4'
output_filepath = 'result'

fs = 44100
seconds = 10


def mp3_to_wav(audio_file_name):
    if audio_file_name.split('.')[1] == 'mp3':
        sound = AudioSegment.from_mp3(audio_file_name)
        audio_file_name = audio_file_name.split('.')[0] + '.wav'
        sound.export(audio_file_name, format="wav")
    return audio_file_name


def stereo_to_mono(audio_file_name):
    sound = AudioSegment.from_wav(audio_file_name)
    sound = sound.set_channels(1)
    sound.export(audio_file_name, format="wav")


def frame_rate_channel(audio_file_name):
    with wave.open(audio_file_name, "rb") as wave_file:
        frame_rate = wave_file.getframerate()
        channels = wave_file.getnchannels()
        return frame_rate, channels


def upload_blob(bucket_name, source_file_name, destination_blob_name):
    client = storage.Client.from_service_account_json('keys/nice-beanbag-290719-c301cd8d4596.json')
    bucket = client.get_bucket(bucket_name)
    blob = bucket.blob(destination_blob_name)

    blob.upload_from_filename(source_file_name)
    return blob.name


def delete_blob(bucket_name, blob_name):
    client = storage.Client.from_service_account_json('keys/nice-beanbag-290719-c301cd8d4596.json')
    bucket = client.get_bucket(bucket_name)
    blob = bucket.blob(blob_name)

    blob.delete()


def google_transcribe(audio_file_name):
    file_name = filepath + audio_file_name
    file_name = mp3_to_wav(file_name)
    print("Converted to .wav")
    return transcribe(file_name, audio_file_name)


def speech_transcribe():
    print(f'Start recording for {seconds} seconds')
    myrecording = sd.rec(int(seconds * fs), samplerate=fs, channels=2, dtype=np.int16)
    sd.wait()
    print('Recording is done')
    orig_filename = 'output.wav'
    file_name = filepath + orig_filename
    write(file_name, fs, myrecording)
    print('Processing...')
    return transcribe(file_name, orig_filename)


def transcribe(file_name, orig_filename):
    frame_rate, channels = frame_rate_channel(file_name)
    print("Get framerate and channels")
    if channels > 1:
        stereo_to_mono(file_name)

    bucket_name = bucketname
    source_file_name = file_name
    destination_blob_name = orig_filename.split('.')[0] + '.wav'

    upload_blob(bucket_name, source_file_name, destination_blob_name)

    print("Uploaded to cloud")

    gcs_uri = 'gs://' + bucketname + '/' + destination_blob_name

    transcript = ''

    client = speech.SpeechClient.from_service_account_json('keys/nice-beanbag-290719-c301cd8d4596.json')
    audio = speech.RecognitionAudio(uri=gcs_uri)

    config = speech.RecognitionConfig(
        encoding=speech.RecognitionConfig.AudioEncoding.LINEAR16,
        sample_rate_hertz=frame_rate,
        language_code="en-US",
    )

    operation = client.long_running_recognize(config=config, audio=audio)
    response = operation.result(timeout=10000)
    print("Got result")
    for result in response.results:
        transcript += result.alternatives[0].transcript
        print(transcript)

    delete_blob(bucket_name, destination_blob_name)
    return transcript


def write_transcripts(transcript_filename, transcript):
    f = open(output_filepath + transcript_filename, "w+")
    f.write(transcript)
    f.close()


if __name__ == '__main__':
    option = input('Your voice[y]/pre-recorded sample[p]')
    if option == 'p':
        for audio_filename in os.listdir(filepath):
            if audio_filename.split('.')[1] == 'wav':
                os.remove(filepath + audio_filename)
                continue
            transcript = google_transcribe(audio_filename)
            transcript_filename = audio_filename.split('.')[0] + '.txt'
            write_transcripts(transcript_filename, transcript)
    elif option == 'y':
        speech_transcribe()
    else:
        print('No option')
