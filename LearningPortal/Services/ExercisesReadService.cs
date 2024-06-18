using System.Text;
using System.Text.RegularExpressions;
using LearningPortal.DataBaseTables;

namespace LearningPortal.Services
{
    public class ExercisesReadService
    {
        private readonly ILogger<ExercisesReadService> _logger;
        private readonly ApplicationContext _db;

        public ExercisesReadService(ILogger<ExercisesReadService> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public void ReadData()
        {
            string path = Directory.GetCurrentDirectory() + @"\wwwroot\tasks\";
            string[] allFiles = Directory.GetFiles(path);
            _logger.LogInformation($"Start reading files in {path}.");

            string pattern = @"^(?<type>[0-9]+)t(?<lvl>[0-9]+)d(?<num>[0-9]+)$";

            foreach (string file in allFiles)
            {
                if (Path.GetExtension(file) == ".txt")
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    Match match = Regex.Match(name, pattern);

                    if (match.Success)
                    {
                        int type = int.Parse(match.Groups["type"].Value);
                        int lvl = int.Parse(match.Groups["lvl"].Value);
                        int num = int.Parse(match.Groups["num"].Value);

                        StringBuilder text = new StringBuilder();
                        string answer = "";
                        using (StreamReader reader = new StreamReader(file))
                        {
                            string? line;
                            while ((line = reader.ReadLine())!.Contains("Ответ: ") == false)
                            {
                                text.AppendLine(line + "<br />");
                            }
                            do
                            {
                                string newLine = line;
                                if (line.Contains("Ответ:"))
                                {
                                    newLine = newLine.Remove(0, 7);
                                }
                                if (line.Contains("."))
                                {
                                    newLine = newLine.Replace(".", "");
                                }
                                answer += newLine + "\n";
                            } while ((line = reader.ReadLine()) != null);
                        }

                        Exercise exercise = new Exercise()
                        {
                            Type = type,
                            DifficultyLevel = lvl,
                            Text = text.ToString(),
                            Answer = answer
                        };

                        _db.Exercises.Add(exercise);
                        _db.SaveChanges();

                        _logger.LogInformation($"Added task {name}.");

                        List<string> additionalFiles = allFiles
                            .Where(f => Path.GetFileName(f).StartsWith(name) && Path.GetExtension(f) != Path.GetExtension(file))
                            .ToList();

                        foreach (string f in additionalFiles)
                        {
                            ExerciseFile additionalFile = new ExerciseFile()
                            {
                                Exercise = exercise,
                                Path = Path.GetFileName(f),
                                Extension = Path.GetExtension(f)
                            };

                            _db.ExerciseFiles.Add(additionalFile);
                            _logger.LogInformation($"Added {Path.GetExtension(f)} file for task {name}.");
                        }

                        _db.SaveChanges();
                    }
                    else
                    {
                        _logger.LogError($"File {file} doesn't match the pattern.");
                    }
                }
            }

            _logger.LogInformation("Exercises database has been created.");
        }
    }
}