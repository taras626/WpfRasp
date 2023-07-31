using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using WpfRasp.Models;

namespace WpfRasp
{
    public static class ExcelWriter
    {
        private const string TYPEOFFILE = ".xlsx";
        private const int COUNTOFLINES = 7; // Кол-во строк содержащихся в каждом дне в таблице Excel 
        public static void CreateResult(DateTime date, Faculty faculty) 
        {
            if (faculty == null) 
            {
                Console.WriteLine("Данные о факультете не найдены");
                Environment.Exit(0);
            }

            using (var excelPackage = new ExcelPackage()) 
            {
                var sheet = excelPackage.Workbook.Worksheets.Add("Расписание");
                sheet.Column(1).Width = 22;
                sheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells.Style.WrapText = true;

                int countOfWeek = 0;
                int countOfLesson = 1;
                DateTime dateOfLesson = new DateTime(date.Year, date.Month, date.Day);
                int countOfDayInWeek = dateOfLesson.Day;
                int allLines = DateTimeManager.GetCountOfDays(date) * COUNTOFLINES;
                int stepForDayInWeek = -1;
                bool isADayLine = false;

                int countOfHalfGroups = 0;

                for (int j = 1; j <= allLines; j++) // Перебор строк
                {
                    for (int i = 1; i <= faculty.Groups.Count() + 1; i++)//Перебор столбцов
                    {
                        if (i != 1)
                            sheet.Column(i).Width = 17;

                        if (j == 1) 
                        {
                            if(i != 1) //Подпись групп
                            {
                                string facultyName = $"{faculty.Groups[i - 2].label}";
                                //if (faculty.Groups[i - 2].lectures.FirstOrDefault(x => x.subGroup != null) != null) 
                                //{
                                //    countOfHalfGroups++;
                                //    //Расписать вывод шапок столбцов по подгруппам
                                //}(1 + countOfHalfGroups*2)
                                sheet.Cells[j, i].Value = facultyName;
                                sheet.Cells[j, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                sheet.Cells[j, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            }
                        }

                        if (i == 1) // Первый столбец
                        {
                            if (j == 1 || (int)dateOfLesson.DayOfWeek == 0 && countOfLesson == 0) //Подпись для недели
                            {

                                countOfWeek++;
                                allLines++;

                                sheet.Cells[j, i].Value = $"Неделя {countOfWeek}";
                                sheet.Cells[j, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                sheet.Cells[j, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                if (j != 1)
                                    dateOfLesson = dateOfLesson.AddDays(1);

                                countOfLesson = 1;
                                countOfDayInWeek = 0;
                                isADayLine = true;
                            }
                            else
                            {
                                stepForDayInWeek = countOfWeek * 2 > COUNTOFLINES ? countOfWeek * 2 - 7 : countOfWeek * 2;
                                if ((j - stepForDayInWeek) % 7 == 0)// Отображение дня недели и даты
                                {
                                    sheet.Cells[j, i].Value = $"{dateOfLesson.ToString("ddd").ToUpperInvariant()}, {dateOfLesson.ToString("dd.MM.yy")}";
                                    sheet.Cells[j, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                    countOfDayInWeek++;
                                    countOfLesson = 1;
                                    isADayLine = true;

                                }
                                else //Отображение времени расписания
                                {
                                    isADayLine = false;
                                    sheet.Cells[j, i].Value = $"{ConfigManager.config.TimeOfLessons[countOfLesson - 1]}";
                                    sheet.Cells[j, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                    if (countOfLesson == 6)
                                        sheet.Cells[j, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                }
                            }
                        }
                        else
                        {
                            if (j != 1) //Заполнение таблицы
                            {
                                if (countOfLesson != 0 && !isADayLine)
                                {
                                    Lecture lecture = faculty.Groups[i - 2].lectures
                                        .Where(x => x.date.Equals(dateOfLesson.ToString("yyyy.MM.dd")))
                                        .ToList()
                                        .FirstOrDefault(y => y.beginLesson == ConfigManager.config.TimeOfLessons[countOfLesson - 1]);

                                    if (lecture != null)
                                        sheet.Cells[j, i].Value = lecture.building;
                                    else
                                    { 
                                        sheet.Cells[j, i].Value = $"Окно";
                                        sheet.Cells[j, i].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        sheet.Cells[j, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#99FF97"));
                                    }

                                    sheet.Cells[j, i].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                                    if (countOfLesson == 1)
                                        sheet.Cells[j, i].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                    if (countOfLesson == 6)                                    
                                        sheet.Cells[j, i].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                        
                                    if (i == faculty.Groups.Count() + 1) 
                                    {
                                        if (countOfLesson == 6)
                                        {
                                            countOfLesson = 0;
                                            dateOfLesson = dateOfLesson.AddDays(1);
                                            if ((int)dateOfLesson.DayOfWeek == 0)
                                            {
                                                j++;
                                                allLines++;
                                                allLines -= COUNTOFLINES;
                                            }
                                        }
                                        else
                                            countOfLesson++;
                                    }
                                }
                            }
                        }
                    }    
                }
                string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}\\{ConfigManager.config.FacultiesNames.GetValueOrDefault(faculty.Name)}_{date.ToString("MMMyyyy")}{TYPEOFFILE}";
                File.WriteAllBytes($"{path}{TYPEOFFILE}", excelPackage.GetAsByteArray());
            }
        }
    }
}
