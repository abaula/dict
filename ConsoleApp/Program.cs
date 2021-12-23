using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace ConsoleApp
{
    class Program
    {
        private const string BasePath = @"E:\Github\dict";

        static void Main()
        {
            //PrintSome();
            //MakeIndex();
            //CreateTrigrams();
            //OpenTrigrams();
            //BuildAndSerializeTwoGramIndex();
            //MakeTwoGramIndex();
            CreateIndex();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static void MakeIndex()
        {
            var index = CreateIndexOld();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var memoryMb = ConvertBytesToMegabytes(GC.GetTotalMemory(true));
            var nodesCount = index.GetNodesCount();

            Console.WriteLine($"Index size {memoryMb} Mb.");
            Console.WriteLine($"Index contains {nodesCount} nodes.");

        }

        static void PrintSome()
        {
            Console.WriteLine($"sizeof char={sizeof(char)}");
            Console.WriteLine($"sizeof short={sizeof(short)}");
            Console.WriteLine($"sizeof int={sizeof(int)}");
            Console.WriteLine($"sizeof byte={sizeof(byte)}");
            Console.WriteLine($"sizeof bool={sizeof(bool)}");
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        private static void BuildAndSerializeTwoGramIndex()
        {
            var words = GetWords($"{BasePath}/hagen_morph/dic.txt");
            var builder = new IndexBuilderOld();
            var index = builder.Build(words);
            Serialize(index, "TwoGramIndex.stm");
        }

        private static void MakeTwoGramIndex()
        {
            var query = "шляпа документы документ краска краской jkhdsf фиолетово";
            //var query = "документы";

            //BuildAndSerializeTwoGramIndex();
            //return;
            var index = Deserialize<IndexOld>("TwoGramIndex.stm");

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var memoryMb = ConvertBytesToMegabytes(GC.GetTotalMemory(true));
            Console.WriteLine($"Index size {memoryMb} Mb.");

            // TODO проверка в индексе

            var words = query.Split(" ").Select(w => w.Trim()).Where(w => !string.IsNullOrEmpty(w)).ToArray();
            var checkTimes = 10000;
            var stopwatch = Stopwatch.StartNew();

            for (var i = 0; i < checkTimes; i++)
            {
                foreach (var word in words)
                    index.Check(word);
            }

            stopwatch.Stop();
            var delta = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds);
            Console.WriteLine($"Проверка {checkTimes} раз. Всего {delta}. {(delta / checkTimes)} для одного");
            Console.WriteLine();

            foreach (var word in words)
            {
                var res = index.Check(word);
                Console.WriteLine($"{word} - {res}");
            }

            // TODO поиск в индексе с джокером


            // TODO предложения


        }

        private static TextGroup[] CreateTextGroupsFromFile(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var groups = new List<TextGroup>();
                var groupWords = new List<string>();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        if (!groupWords.Any())
                            continue;

                        var textGroup = new TextGroup
                        {
                            Sequences = groupWords.Select(_ => new TextSequence { Sequence = _ }).ToArray()
                        };

                        groups.Add(textGroup);
                        groupWords.Clear();

                        continue;
                    }

                    var arr = line.Split('|')
                        .Select(w => w.Trim())
                        .Where(w => !string.IsNullOrEmpty(w))
                        .ToArray();

                    if (!arr.Any())
                        continue;

                    groupWords.Add(arr[0].ToLower());
                }

                return groups.ToArray();
            }
        }

        private static IIndex CreateIndex()
        {
            var groups = CreateTextGroupsFromFile($"{BasePath}/hagen_morph/dic.txt");
            var indexBuilder = new IndexBuilder();
            var index = indexBuilder.Build(groups);

            return index;
        }

        private static NodeIndex CreateIndexOld()
        {
            //var words = GetWords($"{BasePath}/hagen_morph/dic_short.txt");
            var words = GetWords($"{BasePath}/hagen_morph/dic.txt");
            var index = new NodeIndex();

            foreach (var word in words)
            {
                index.Add(word);
            }

            return index;
        }

        static void OpenTrigrams()
        {
            var trigrams = Deserialize<NgramOld[]>("trigrams_array.stm");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            var memoryMb = ConvertBytesToMegabytes(GC.GetTotalMemory(true));
            Console.WriteLine($"Used memory {memoryMb} Mb.");
            Console.WriteLine($"File contains {trigrams.Length} trigrams");
        }

        static void CreateTrigrams()
        {
            var words = GetWords($"{BasePath}/hagen_morph/dic.txt");
            var ngrams = new List<string>();
            var gramsFreqMap = new Dictionary<string, NgramOld>();
            var ggramsFreqMap = new Dictionary<string, NgramOld>();
            var charsMap = new Dictionary<string, NgramOld>();

            void UpdateMap(Dictionary<string, NgramOld> map, string key)
            {
                var ngram = map.ContainsKey(key)
                    ? map[key]
                    : new NgramOld
                    {
                        Gram = key,
                        Frequency = 0,
                        Percent = 0f
                    };

                ++ngram.Frequency;
                map[key] = ngram;
            }

            foreach (var word in words)
            {
                foreach (var ch in word)
                    UpdateMap(charsMap, ch.ToString());

                if (word.Length < 3)
                {
                    UpdateMap(gramsFreqMap, word);
                    UpdateMap(ggramsFreqMap, word);
                    continue;
                }

                ngrams.Clear();

                for (var start = 0; start < word.Length - 2; start++)
                {
                    var ngram = word.Substring(start, 3);
                    UpdateMap(gramsFreqMap, ngram);
                    ngrams.Add(ngram);
                }

                if (ngrams.Count < 4)
                {
                    UpdateMap(ggramsFreqMap, $"{ngrams.First()}-");
                    continue;
                }

                for (var i = 0; i < ngrams.Count - 3; i++)
                for (var j = i + 3; j < ngrams.Count; j++)
                {
                    UpdateMap(ggramsFreqMap, $"{ngrams[i]}|{ngrams[j]}");
                }
            }

            void UpdateFrequency(Dictionary<string, NgramOld> map)
            {
                var totalFrequency = map.Values.Sum(ng => ng.Frequency);

                foreach (var ngram in map.Values)
                    ngram.Percent = (float)ngram.Frequency / totalFrequency;
            }

            UpdateFrequency(charsMap);

            const int quantity = 100;
            Console.WriteLine($"Created {charsMap.Count} chars");

            foreach (var ngram in charsMap.Values.OrderByDescending(pv => pv.Percent))
                Console.WriteLine($"\t{ngram.Gram}, freq: {ngram.Frequency}, prc: {(ngram.Percent * 100):f4}% ");

            Console.WriteLine();

            UpdateFrequency(gramsFreqMap);

            Serialize(gramsFreqMap, "trigrams_map.stm");
            var trigrams = gramsFreqMap.Values.ToArray();
            Serialize(trigrams, "trigrams_array.stm");

            Console.WriteLine($"Created {gramsFreqMap.Count} trigrams");

            Console.WriteLine($"Top {quantity} trigrams");

            foreach(var ngram in trigrams.OrderByDescending(pv => pv.Percent).Take(quantity))
                Console.WriteLine($"\t{ngram.Gram}, freq: {ngram.Frequency}, prc: {(ngram.Percent * 100):f4}% ");


            Console.WriteLine();
            UpdateFrequency(ggramsFreqMap);

            Serialize(ggramsFreqMap, "two_trigrams_map.stm");
            var twoTrigrams = ggramsFreqMap.Values.ToArray();
            Serialize(twoTrigrams, "two_trigrams_array.stm");

            Console.WriteLine($"Created {ggramsFreqMap.Count} two trigrams");
            Console.WriteLine($"Top {quantity} two trigrams");

            foreach (var ngram in twoTrigrams.OrderByDescending(pv => pv.Percent).Take(quantity))
                Console.WriteLine($"\t{ngram.Gram}, freq: {ngram.Frequency}, prc: {(ngram.Percent * 100):f6}% ");
        }

        private static HashSet<string> GetWords(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var words = new HashSet<string>();

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine()?.Trim();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var arr = line.Split('|')
                        .Select(w => w.Trim())
                        .Where(w => !string.IsNullOrEmpty(w))
                        .ToArray();

                    if (!arr.Any())
                        continue;

                    words.Add(arr[0].ToLower());
                }

                return words;
            }
        }

        static void Serialize(object data, string fileName)
        {
            var formatter = new BinaryFormatter();

            using (var fs = new FileStream(Path.Combine($"{BasePath}/data", fileName), FileMode.Create))
            {
                formatter.Serialize(fs, data);
            }
        }

        static T Deserialize<T>(string fileName)
        {
            var formatter = new BinaryFormatter();

            using (var fs = new FileStream(Path.Combine($"{BasePath}/data", fileName), FileMode.Open))
            {
                return (T)formatter.Deserialize(fs);
            }
        }
    }
}
