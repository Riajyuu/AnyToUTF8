#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using Ude;
using Ude.Core;

namespace ConvertConsole {
    internal static class Program {
        private static Encoding? _sourceEncoding;
        private static string? _settingPath;

        private static void Main(string[] args) {
#pragma warning disable CS8604 // 可能有 Null 参数
            _settingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.ChangeExtension("setting", "xst"));
#pragma warning restore CS8604 // 可能有 Null 参数
            if (!File.Exists(_settingPath)) {
                File.WriteAllText(_settingPath, $"auto{Environment.NewLine}txt");
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (args.Length is 0) {
                ;
            start:
                Console.Write($"1.Convert File Encoding.{Environment.NewLine}2.Edit Setting.{Environment.NewLine}3.Exit{Environment.NewLine}Input Number to Choose Action:");
                if (!int.TryParse(Console.ReadLine(), out var result)) {
                    goto start;
                }
                switch (result) {
                    case 1:
                        Console.WriteLine("Input Path to convert:");
                        Convert(Console.ReadLine(), false);
                        break;
                    case 2:
                        EditSetting();
                        goto start;
                    case 3:
                        return;
                    default:
                        goto start;
                }
            }
            else Convert(args[0], true);
        }

        private static void Convert(string path, bool isDrag) {
            try {
                var inp = File.ReadAllLines(_settingPath);
                var textBites = File.ReadAllBytes(path);
                if (string.Compare(inp[0], "auto", StringComparison.OrdinalIgnoreCase) is 0) {
                    var detector = new CharsetDetector();
                    detector.Feed(textBites, 0, textBites.Length);
                    detector.DataEnd();
                    _sourceEncoding = Encoding.GetEncoding(CharsetPageConvert(detector.Charset));
                }
                else {
                    _sourceEncoding = int.TryParse(inp[0], out var result) ? Encoding.GetEncoding(result) : Encoding.GetEncoding(inp[0]);
                }
                textBites = Encoding.Convert(_sourceEncoding, Encoding.UTF8, textBites);
                File.WriteAllBytes(Path.ChangeExtension(path, inp[1]), textBites);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        private static void EditSetting() {
            //var setting = new List<string>();
            Console.WriteLine("You Can enter the encoding property name(such as: gb2312, shift_jis), code page(such as: 936, 932) or auto. The default value is auto.");
        inputEncoding:
            Console.Write("Source Encoding:");
            var inp = Console.ReadLine();
            var sets = new List<String>();
            if (!(string.Compare(inp, "auto", StringComparison.OrdinalIgnoreCase) is 0)) {
                try {
                    _sourceEncoding = int.TryParse(inp, out var result) ? Encoding.GetEncoding(result) : Encoding.GetEncoding(inp);
                }
                catch (Exception ex) {
                    Console.WriteLine(ex.Message);
                    goto inputEncoding;
                }
            }
            sets.Add(inp);
            Console.WriteLine("Enter File Extension name(e.g. \"txt\" to *.txt):");
            var re = false;
            do {
                inp = Console.ReadLine();
                if (inp?.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
                    re = true;
                }
            } while (re);
            if (inp != null) sets.Add(inp);
            try {
                File.WriteAllLines(_settingPath, sets);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                goto inputEncoding;
            }
        }

        private static string CharsetPageConvert(string charset) {
            return charset switch
            {
                Charsets.ASCII => "us-ascii",
                Charsets.BIG5 => "big5",
                Charsets.EUCJP => "EUC-JP",
                Charsets.EUCKR => "euc-kr",
                Charsets.EUCTW => "euc-tw",
                Charsets.GB18030 => "GB18030",
                Charsets.HZ_GB_2312 => "hz-gb-2312",
                Charsets.IBM855 => "IBM855",
                Charsets.IBM866 => "IBM866",
                Charsets.ISO2022_CN => "iso-2022-cn",
                Charsets.ISO2022_JP => "iso-2022-jp",
                Charsets.ISO2022_KR => "iso-2022-kr",
                Charsets.UTF8 => "utf-8",
                Charsets.UTF16_LE => "utf-16",
                // ReSharper disable once StringLiteralTypo
                Charsets.UTF16_BE => "unicodeFFFE",
                Charsets.UTF32_BE => "utf-32BE",
                Charsets.UTF32_LE => "utf-32",
                Charsets.UCS4_2413 => "",
                Charsets.UCS4_3412 => "",
                Charsets.WIN1251 => "windows-1251",
                Charsets.WIN1252 => "Windows-1252",
                Charsets.WIN1253 => "windows-1253",
                Charsets.WIN1255 => "windows-1255",
                Charsets.SHIFT_JIS => "shift_jis",
                Charsets.MAC_CYRILLIC => "x-mac-cyrillic",
                Charsets.KOI8R => "koi8-r",
                Charsets.ISO8859_2 => "	iso-8859-2",
                Charsets.ISO8859_5 => "iso-8859-5",
                Charsets.ISO_8859_7 => "	iso-8859-7",
                Charsets.ISO8859_8 => "iso-8859-8",
                Charsets.TIS620 => "",
                _ => string.Empty,
            };
        }
    }
}
