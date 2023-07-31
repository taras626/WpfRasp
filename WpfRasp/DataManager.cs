using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfRasp.Models;

namespace WpfRasp
{
    internal static class DataManager
    {
        public static List<Faculty> Faculties { get; set; }

        private const string URLFORSEARCH = "https://rasp.omgtu.ru/api/";
        private const string DATEFORMAT = "yyyy.MM.dd";

        private static HttpClient httpClient = new HttpClient();

        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = true
        };

        private static async Task<List<Lecture>> GetLecturesAsync(DateTime dateTime, int idGroup) 
        {
            DateTime begin = new DateTime(dateTime.Year, dateTime.Month, 1);
            DateTime end = new DateTime(dateTime.Year, dateTime.Month, DateTimeManager.GetCountOfDays(dateTime));

            using HttpRequestMessage requestForLecture = new HttpRequestMessage(HttpMethod.Get, $"{URLFORSEARCH}schedule/group/{idGroup}?start={begin.ToString(DATEFORMAT)}&finish={end.ToString(DATEFORMAT)}&lng=1");

            // получаем ответ
            using HttpResponseMessage responseForLecture = await httpClient.SendAsync(requestForLecture);

            //Конвертируем запрос в строку
            string content = await responseForLecture.Content.ReadAsStringAsync();

            List<Lecture> LecturesForGroup = JsonSerializer.Deserialize<List<Lecture>>(content);

            return LecturesForGroup;
        }

        private static int ReadLocalData(string url) 
        {
            using (StreamReader reader = new StreamReader(url))
            {
                string data = reader.ReadToEnd();
                Faculties = JsonSerializer.Deserialize<List<Faculty>>(data);
                if(Faculties == null)
                    return -1;
                return 0;
            }
        }

        public static async Task<Faculty> GetAllDataFaculty(InputRequest inputRequest, string filePathFaculties)
        {
            if (Faculties == null)
                if (ReadLocalData(filePathFaculties) == -1)
                    return null;

            Faculty faculty = Faculties.First(x => x.Name == inputRequest.FacultyName);

            List<Group> groups = faculty.Groups;

            for (int i = 0; i < groups.Count(); i++) 
            {
                groups[i].lectures = await GetLecturesAsync(inputRequest.MonthOfLessons, groups[i].id);
            }

            var t = 0;

            return faculty;
        }

        /// <summary>
        /// Добавляет новую группу в локальную память
        /// </summary>
        /// <param name="groupName">Имя группы</param>
        public static async Task AddNewGroupAsync(string groupName) 
        {
            using HttpRequestMessage requestForId = new HttpRequestMessage(HttpMethod.Get, $"{URLFORSEARCH}search?term={groupName}&type=group");
            // получаем ответ
            using HttpResponseMessage responseForId = await httpClient.SendAsync(requestForId);
            //Конвертируем запрос в строку

            if (responseForId == null)
            {
                MessageBox.Show("Ошибка, сервер не доступен или вы ввели неправильную группу!");                
            }
            else 
            {
                string? content = await responseForId.Content.ReadAsStringAsync();

                List<Group>? groups = JsonSerializer.Deserialize<List<Group>>(content);

                if (groups.Count == 0)
                {
                    MessageBox.Show("Ошибка, сервер отправил не корректные данные, проверьте файл с группами, который вы выбрали.");
                }
                else
                {
                    Group group = groups.First(x => x.label == groupName);

                    List<Faculty>? faculties;

                    using (StreamReader reader = new StreamReader(ConfigManager.config.PathFaculty))
                    {
                        string data = reader.ReadToEnd();

                        faculties = data.Equals("") ? new List<Faculty>() : JsonSerializer.Deserialize<List<Faculty>>(data);
                        Faculty fac = data.Equals("") ? null : faculties.FirstOrDefault(x => x.Name == group.description);

                        if (fac == null)
                        {
                            faculties.Add(new Faculty()
                            {
                                Name = group.description,
                                Groups = new List<Group> {
                                new Group()
                                {
                                    label = group.label,
                                    id = group.id
                                }
                            }
                            });
                            if (ConfigManager.config.FacultiesNames.FirstOrDefault(x => x.Key.Equals(group.description)).Key == null) 
                            {
                                ConfigManager.EditFacultiesConf(group.description);
                                SaveResultJson(ConfigManager.CONFIGPATH, ConfigManager.config);
                            }
                        }
                        else
                        {
                            if (
                                fac.Groups.FirstOrDefault(x => x.label.Equals(groupName)) == null
                                )
                            {
                                fac.Groups.Add(new Group()
                                {
                                    label = group.label,
                                    id = group.id
                                });
                            }
                            else
                            {
                                MessageBox.Show($"Вы ввели группу({groupName}), которая уже добавлена!");
                            }

                        }
                    }
                    SaveResultJson(ConfigManager.config.PathFaculty, faculties);
                    Faculties = faculties;
                    
                    //writer.WriteLine(JsonSerializer.Serialize(faculties, options));
                    
                }
            }
        }

        public static void DeleteGroups() 
        {
            File.WriteAllText(ConfigManager.config.PathFaculty, string.Empty);
        }

        public static void DeleteGroup(string groupName) 
        {
            foreach (Faculty faculty in Faculties) 
            {
                faculty.Groups.Remove(faculty.Groups.FirstOrDefault(x => x.label.Equals(groupName)));
            }

            using (StreamWriter writer = new StreamWriter(ConfigManager.config.PathFaculty))
            {
                writer.WriteLine(JsonSerializer.Serialize(Faculties, options));
            }
        }

        public static void DeleteFaculty(string facultyName) 
        {
            Faculties.Remove(Faculties.FirstOrDefault(x => x.Name.Equals(facultyName)));
            using (StreamWriter writer = new StreamWriter(ConfigManager.config.PathFaculty))
            {
                writer.WriteLine(JsonSerializer.Serialize(Faculties, options));
            }
        }

        public static Faculty GetFacultyByShortName(string shortName)
        {
            return Faculties.FirstOrDefault(x => x.Name.Equals(ConfigManager.config.FacultiesNames.FirstOrDefault(y => y.Value.Equals(shortName)).Key));
        }

        public static void SaveResultJson(string filePath, object data) 
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(JsonSerializer.Serialize(data, options));
            }
        }
    }
}
