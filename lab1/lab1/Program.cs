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

            Dictionary<Data, float> totalWordsCollection = new Dictionary<Data, float>();
            Dictionary<Data, float> spamWordsCollection = new Dictionary<Data, float>();
            Dictionary<Data, float> notSpamWordsCollection = new Dictionary<Data, float>();

            int i_notSpam = notSpamWords.Count / 3;
            int i_spam = spamWords.Count / 3;

            ArrayList testData = new ArrayList();
            for (int i = i_notSpam; i < notSpamWords.Count; i++)
            {
                var data = new Data() { Value = notSpamWords[i].ToString(), Type = DataType.NOT_SPAM };
                testData.Add(data);
            }
            for (int i = i_spam; i < spamWords.Count; i++)
            {
                var data = new Data() { Value = spamWords[i].ToString(), Type = DataType.SPAM};
                testData.Add(data);
            }


            for (int i = 0; i < i_notSpam; i++)
            {
                String[] _notSpamWords = notSpamWords[i].ToString().Split(delimiterChars);

                foreach (string word in _notSpamWords)
                {
                    if (word == "")
                    {
                        continue;
                    }

                    //spamWordsCollection.TryAdd(word.ToLower(), 0);

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

            for (int i = 0; i < i_spam; i++)
            {
                String[] _spamWords = spamWords[i].ToString().Split(delimiterChars);
                foreach (string word in _spamWords)
                {
                    if (word == "")
                    {
                        continue;
                    }

                    //notSpamWordsCollection.TryAdd(word.ToLower(), 0);

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

            Dictionary<Data, float> notSpamWordsProbability = new Dictionary<Data, float>();
            Dictionary<Data, float> spamWordsProbability = new Dictionary<Data, float>();

            foreach (var item in notSpamWordsCollection)
            {
                notSpamWordsProbability[item.Key] =
                    (totalWordsCollection[item.Key] * (notSpamWordsCollection[item.Key] / totalWordsCollection[item.Key]) + 0.5f) /
                    (totalWordsCollection[item.Key] + 1.0f);
            }

            foreach (var item in spamWordsCollection)
            {
                spamWordsProbability[item.Key] =
                    (totalWordsCollection[item.Key] * (spamWordsCollection[item.Key] / totalWordsCollection[item.Key]) + 0.5f) /
                    (totalWordsCollection[item.Key] + 1.0f);
            }

            int detectedSpamCount = 0;
            int detectedNotSpamCount = 0;
            int newWordsMessageCount = 0;


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


                //Console.Write(notSpamWords[i].ToString() + ": ");

                if (spamProbability > notSpamProbability)
                {
                    //Console.WriteLine("It is spam");
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

            

            Console.WriteLine("Detected spam out of: " + detectedSpamCount + "/" + (spamWords.Count - i_spam));
            Console.WriteLine("Detected not spam out of: " + detectedNotSpamCount + "/" + (notSpamWords.Count - i_notSpam));
            Console.WriteLine("Total words: " + testData.Count);
            Console.WriteLine("Building diagram...");

            float a = detectedSpamCount;
            float b = (spamWords.Count - i_spam);

            float coef = a / b;
            run_cmd("main.py", String.Format("{0} {1}", coef, b));


        }

        private static void run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = "python.exe";
            start.Arguments = string.Format("{0} {1}", cmd, args);
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

        static void ReadCsvFile<E>(string pathToFile, ArrayList appendingWordsList, int itemCount = 101) where E : BaseModel
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
