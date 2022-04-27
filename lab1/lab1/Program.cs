using CsvHelper;
using lab1.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace lab1
{
    enum DataType { SPAM, NOT_SPAM }

    struct Data
    {
        public string Value { get; set; }
        public DataType Type { get; set; }
    }

    class ViewModel 
    {
        public int InputCount { get; set; }
        public float Accuracy { get; set; }
    }

    internal class Program
    {
        static char[] delimiterChars = { ' ', ',', '.', ':', '\t', '{', '}', '[', ']', '/', '*', '\'', '\"', '+', '-', '#', '!', '?' };

        static async Task Main(string[] args)
        {
            Console.WriteLine("Analyzing...");


            ArrayList notSpamWords = ArrayList.Synchronized(new ArrayList());
            ArrayList spamWords = ArrayList.Synchronized(new ArrayList());

            ReadCsvFile<AutoModel>(pathToFile: "Data/auto.csv", appendingWordsList: notSpamWords);
            ReadCsvFile<TheatreModel>(pathToFile: "Data/theatre.csv", appendingWordsList: notSpamWords);
            ReadCsvFile<InternetModel>(pathToFile: "Data/internet.csv", appendingWordsList: notSpamWords);

            ReadCsvFile<Spam1Model>(pathToFile: "Data/spam_1.csv", appendingWordsList: spamWords);
            ReadCsvFile<Spam2Model>(pathToFile: "Data/spam_2.csv", appendingWordsList: spamWords);


            Shuffle(notSpamWords);
            Shuffle(spamWords);

            int i_notSpam = notSpamWords.Count / 3;
            int i_spam = spamWords.Count / 3;

            ArrayList testData = new ArrayList();
            InitializeTestData(notSpamWords, spamWords, i_notSpam, i_spam, testData);

            ArrayList viewModels = new ArrayList();

            for (int i = 0; i < Math.Max(i_notSpam, i_spam); i += 1000)
            {
                int spamThreshold = i > i_spam ? i_spam : i;
                int notSpamThreshold = i > i_notSpam ? i_notSpam : i;

                Dictionary<Data, float> totalWordsCollection
                    = new Dictionary<Data, float>();
                Dictionary<Data, float> spamWordsCollection
                    = new Dictionary<Data, float>();
                Dictionary<Data, float> notSpamWordsCollection
                    = new Dictionary<Data, float>();



                NotSpamWordsCounting(notSpamWords, totalWordsCollection, notSpamWordsCollection, notSpamThreshold);
                SpamWordsCounting(spamWords, totalWordsCollection, spamWordsCollection, spamThreshold);

                Dictionary<Data, float> notSpamWordsProbability = new Dictionary<Data, float>();
                Dictionary<Data, float> spamWordsProbability = new Dictionary<Data, float>();

                NotSpamWordProbabilityCalc(totalWordsCollection, notSpamWordsCollection, notSpamWordsProbability);
                SpamWordProbabilityCalc(totalWordsCollection, spamWordsCollection, spamWordsProbability);

                int detectedSpamCount = 0;
                int detectedNotSpamCount = 0;
                int newWordsMessageCount = 0;

                DataTesting(
                    testData,
                    notSpamWordsProbability,
                    spamWordsProbability,
                    ref detectedSpamCount,
                    ref detectedNotSpamCount,
                    ref newWordsMessageCount);

                Console.WriteLine("Detected spam out of: " + detectedSpamCount + "/" + (testData.Count - i_notSpam*2));
                Console.WriteLine("Detected not spam out of: " + detectedNotSpamCount + "/" + (testData.Count - i_spam * 2));
                Console.WriteLine("Total words: " + testData.Count);

                float a = detectedSpamCount + detectedNotSpamCount;
                float b = testData.Count;

                float coef = a / b;
                Console.WriteLine("Coef: " + coef);
                viewModels.Add(new ViewModel { InputCount = spamThreshold + notSpamThreshold, Accuracy = coef});
                Console.WriteLine("----------------------------------------------------------------------------------------");
            }

            FileStream file = File.Create("accuracy.txt");
            StreamWriter streamWriter = new StreamWriter(file);
            for (int i = 0; i < viewModels.Count; i++)
            {
                ViewModel v = viewModels[i] as ViewModel;
                streamWriter.WriteLine("accuracy: " + v.Accuracy + "; count: " + v.InputCount);
            }
            streamWriter.Dispose();
            streamWriter.Close();
            file.Dispose ();
            file.Close();
            buildGraph("main.py");

        }

        private static void DataTesting(ArrayList testData, 
            Dictionary<Data, float> notSpamWordsProbability, 
            Dictionary<Data, float> spamWordsProbability, 
            ref int detectedSpamCount, 
            ref int detectedNotSpamCount, 
            ref int newWordsMessageCount)
        {
            for (int i = 0; i < testData.Count; i++)
            {
                Data d = (Data)testData[i];

                String[] words = d.Value.ToString().Split(delimiterChars);

                float spamProbability = 1;
                float notSpamProbability = 1;
                bool wordIsFind = false;
                foreach (var word in words)
                {
                    float notSpamWordProbability = 1;

                    var notSpamData = new Data { Value = word.ToLower(), Type = DataType.NOT_SPAM };
                    var spamData = new Data { Value = word.ToLower(), Type = DataType.SPAM };

                    if (notSpamWordsProbability.TryGetValue(notSpamData, out notSpamWordProbability))
                    {
                        notSpamProbability *= notSpamWordProbability;
                        wordIsFind = true;
                    }

                    float spamWordProbability = 1;
                    if (spamWordsProbability.TryGetValue(spamData, out spamWordProbability))
                    {
                        spamProbability *= spamWordProbability;
                        wordIsFind = true;
                    }
                }

                if (spamProbability == 1)
                    spamProbability = 0;
                if (notSpamProbability == 1)
                    notSpamProbability = 0;

                if (spamProbability > notSpamProbability)
                {
                    if (d.Type == DataType.SPAM)
                        detectedSpamCount++;
                }
                else if (spamProbability < notSpamProbability)
                {
                    if (d.Type == DataType.NOT_SPAM)
                        detectedNotSpamCount++;
                }
                else if (wordIsFind)
                {
                    newWordsMessageCount++;
                }

            }
        }

        private static void SpamWordProbabilityCalc(
            Dictionary<Data, float> totalWordsCollection, 
            Dictionary<Data, float> spamWordsCollection, 
            Dictionary<Data, float> spamWordsProbability)
        {
            foreach (var item in spamWordsCollection)
            {

                spamWordsProbability[item.Key] = normalizingWordProbability(
                    totalWordsCollection[item.Key],
                    spamWordsCollection[item.Key],
                    0.5f);
            }
        }

        private static void NotSpamWordProbabilityCalc(
            Dictionary<Data, float> totalWordsCollection,
            Dictionary<Data, float> notSpamWordsCollection,
            Dictionary<Data, float> notSpamWordsProbability)
        {
            foreach (var item in notSpamWordsCollection)
            {
                notSpamWordsProbability[item.Key] = normalizingWordProbability(
                    totalWordsCollection[item.Key],
                    notSpamWordsCollection[item.Key],
                    0.5f);
            }
        }

        private static float normalizingWordProbability(
            float wordProbabilityGeneral,
            float wordProbabilityClass,
            float classProbability)
        {
            return (wordProbabilityGeneral * (wordProbabilityClass / wordProbabilityGeneral) + 
                classProbability) / (wordProbabilityGeneral + 1.0f);
        }

        private static void SpamWordsCounting(
            ArrayList spamWords, 
            Dictionary<Data, float> totalWordsCollection, 
            Dictionary<Data, float> spamWordsCollection, 
            int spam_data_test_threshold)
        {
            for (int i = 0; i < spam_data_test_threshold; i++)
            {
                String[] _spamWords = spamWords[i].ToString().Split(delimiterChars);
                foreach (string word in _spamWords)
                {
                    if (word == "")
                    {
                        continue;
                    }

                    var data = new Data() { Value = word.ToLower(), Type = DataType.SPAM };

                    if (!totalWordsCollection.TryAdd(data, 1))
                    {
                        totalWordsCollection[data]++;
                    }

                    if (!spamWordsCollection.TryAdd(data, 1))
                    {
                        spamWordsCollection[data]++;
                    }
                }

            }
        }

        private static void NotSpamWordsCounting(
            ArrayList notSpamWords, 
            Dictionary<Data, float> totalWordsCollection, 
            Dictionary<Data, float> notSpamWordsCollection, 
            int notSpam_data_test_threshold)
        {
            for (int i = 0; i < notSpam_data_test_threshold; i++)
            {
                String[] _notSpamWords = notSpamWords[i].ToString().Split(delimiterChars);

                foreach (string word in _notSpamWords)
                {
                    if (word == "")
                    {
                        continue;
                    }

                    var data = new Data() { Value = word.ToLower(), Type = DataType.NOT_SPAM };

                    if (!totalWordsCollection.TryAdd(data, 1))
                    {
                        totalWordsCollection[data]++;
                    }

                    if (!notSpamWordsCollection.TryAdd(data, 1))
                    {
                        notSpamWordsCollection[data]++;
                    }
                }
            }
        }

        private static void InitializeTestData(
            ArrayList notSpamWords, 
            ArrayList spamWords, 
            int notSpam_data_test_threshold, 
            int spam_data_test_threshold, 
            ArrayList testData)
        {
            for (int i = notSpam_data_test_threshold; i < notSpamWords.Count; i++)
            {
                var data = new Data()
                {
                    Value = notSpamWords[i].ToString(),
                    Type = DataType.NOT_SPAM
                };
                testData.Add(data);
            }
            for (int i = spam_data_test_threshold; i < spamWords.Count; i++)
            {
                var data = new Data()
                {
                    Value = spamWords[i].ToString(),
                    Type = DataType.SPAM
                };
                testData.Add(data);
            }
        }

        private static void buildGraph(string cmd)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python.exe";
            start.Arguments = string.Format("{0}", cmd);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        static void ReadCsvFile<E>(string pathToFile, 
            ArrayList appendingWordsList) where E : BaseModel
        {
            using (var reader = new StreamReader(pathToFile))
            {
                using (var csvReader = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    while (csvReader.Read())
                    {
                        E model = csvReader.GetRecord<E>();
                        appendingWordsList.Add(model.Data);
                    }
                }
            }
        }

        static void Shuffle(ArrayList arr)
        {
            Random rand = new Random();
            for (int i = arr.Count - 1; i >= 1; i--)
            {
                int j = rand.Next(i + 1);
                object tmp = arr[j];
                arr[j] = arr[i];
                arr[i] = tmp;
            }
        }
    }
}
