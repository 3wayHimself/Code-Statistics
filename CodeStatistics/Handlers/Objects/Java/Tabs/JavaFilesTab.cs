﻿using CodeStatistics.ConsoleUtil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeStatistics.Handlers.Objects.Java.Tabs{
    class JavaFilesTab : JavaTab{
        const int fileCount = 10;

        private readonly List<KeyValuePair<string,int>> mostLines = new List<KeyValuePair<string,int>>(fileCount);
        private readonly List<KeyValuePair<string,int>> leastLines = new List<KeyValuePair<string,int>>(fileCount);
        private readonly int totalLines, averageLines;

        private readonly List<KeyValuePair<string,long>> mostCharacters = new List<KeyValuePair<string,long>>(fileCount);
        private readonly List<KeyValuePair<string,long>> leastCharacters = new List<KeyValuePair<string,long>>(fileCount);
        private readonly long totalCharacters, averageCharacters;
        
        private readonly List<KeyValuePair<string,int>> mostImports = new List<KeyValuePair<string,int>>(fileCount);
        private readonly List<KeyValuePair<string,int>> leastImports = new List<KeyValuePair<string,int>>(fileCount);
        private readonly int totalImports, averageImports;

        public JavaFilesTab(JavaStatistics stats) : base("Files",stats){
            Dictionary<string,JavaStatistics.JavaFileInfo> dict = stats.FileInfo;

            totalLines = stats.LinesTotal;
            averageLines = totalLines/dict.Count;
            mostLines.AddRange(dict.OrderByDescending(kvp => kvp.Value.Lines).Take(fileCount).Select(kvp => new KeyValuePair<string,int>(JavaParseUtils.GetSimpleName(kvp.Key).Key,kvp.Value.Lines)));
            leastLines.AddRange(dict.OrderBy(kvp => kvp.Value.Lines).Take(fileCount).Select(kvp => new KeyValuePair<string,int>(JavaParseUtils.GetSimpleName(kvp.Key).Key+".java",kvp.Value.Lines)));

            totalCharacters = stats.CharactersTotal;
            averageCharacters = totalCharacters/dict.Count;
            mostCharacters.AddRange(dict.OrderByDescending(kvp => kvp.Value.Characters).Take(fileCount).Select(kvp => new KeyValuePair<string,long>(JavaParseUtils.GetSimpleName(kvp.Key).Key+".java",kvp.Value.Characters)));
            leastCharacters.AddRange(dict.OrderBy(kvp => kvp.Value.Characters).Take(fileCount).Select(kvp => new KeyValuePair<string,long>(JavaParseUtils.GetSimpleName(kvp.Key).Key+".java",kvp.Value.Characters)));

            totalImports = stats.ImportsTotal;
            averageImports = totalImports/dict.Count;
            mostImports.AddRange(dict.OrderByDescending(kvp => kvp.Value.Imports).Take(fileCount).Select(kvp => new KeyValuePair<string,int>(JavaParseUtils.GetSimpleName(kvp.Key).Key+".java",kvp.Value.Imports)));
            leastImports.AddRange(dict.OrderBy(kvp => kvp.Value.Imports).Take(fileCount).Select(kvp => new KeyValuePair<string,int>(JavaParseUtils.GetSimpleName(kvp.Key).Key+".java",kvp.Value.Imports)));
        }

        public override void RenderInfo(ConsoleWrapper c, int y){
            int px, py;

            // Lines
            px = 4;
            py = y;
            c.Write(px,py,"Lines",ConsoleColor.Yellow);
            c.Write(px,++py,"Total: ",ConsoleColor.White);
            c.Write(totalLines.ToString(),ConsoleColor.Gray);
            c.Write(px,++py,"Average: ",ConsoleColor.White);
            c.Write(averageLines.ToString(),ConsoleColor.Gray);
            
            c.Write(px,py += 3,"Most Lines",ConsoleColor.Yellow);

            foreach(KeyValuePair<string,int> kvp in mostLines){ // TODO pad
                c.Write(px,++py,kvp.Key+" - ",ConsoleColor.White);
                c.Write(kvp.Value.ToString(),ConsoleColor.Gray);
            }
            
            c.Write(px,py += 3,"Least Lines",ConsoleColor.Yellow);

            foreach(KeyValuePair<string,int> kvp in leastLines){
                c.Write(px,++py,kvp.Key+" - ",ConsoleColor.White);
                c.Write(kvp.Value.ToString(),ConsoleColor.Gray);
            }

            // Characters
            px = c.Width/2-(3+Math.Max(mostCharacters.Max(kvp => kvp.Key.Length+kvp.Value.ToString().Length),leastCharacters.Max(kvp => kvp.Key.Length+kvp.Value.ToString().Length)))/2;
            py = y;
            c.Write(px,py,"Characters",ConsoleColor.Yellow);
            c.Write(px,++py,"Total: ",ConsoleColor.White);
            c.Write(totalCharacters.ToString(),ConsoleColor.Gray);
            c.Write(px,++py,"Average: ",ConsoleColor.White);
            c.Write(averageCharacters.ToString(),ConsoleColor.Gray);
            
            c.Write(px,py += 3,"Most Characters",ConsoleColor.Yellow);

            foreach(KeyValuePair<string,long> kvp in mostCharacters){
                c.Write(px,++py,kvp.Key+" - ",ConsoleColor.White);
                c.Write(kvp.Value.ToString(),ConsoleColor.Gray);
            }
            
            c.Write(px,py += 3,"Least Characters",ConsoleColor.Yellow);

            foreach(KeyValuePair<string,long> kvp in leastCharacters){
                c.Write(px,++py,kvp.Key+" - ",ConsoleColor.White);
                c.Write(kvp.Value.ToString(),ConsoleColor.Gray);
            }

            // Imports
            px = c.Width-4-(3+Math.Max(mostImports.Max(kvp => kvp.Key.Length+kvp.Value.ToString().Length),leastImports.Max(kvp => kvp.Key.Length+kvp.Value.ToString().Length)));
            py = y;
            c.Write(px,py,"Imports",ConsoleColor.Yellow);
            c.Write(px,++py,"Total: ",ConsoleColor.White);
            c.Write(totalImports.ToString(),ConsoleColor.Gray);
            c.Write(px,++py,"Average: ",ConsoleColor.White);
            c.Write(averageImports.ToString(),ConsoleColor.Gray);
            
            c.Write(px,py += 3,"Most Imports",ConsoleColor.Yellow);

            foreach(KeyValuePair<string,int> kvp in mostImports){
                c.Write(px,++py,kvp.Key+" - ",ConsoleColor.White);
                c.Write(kvp.Value.ToString(),ConsoleColor.Gray);
            }
            
            c.Write(px,py += 3,"Least Imports",ConsoleColor.Yellow);

            foreach(KeyValuePair<string,int> kvp in leastImports){
                c.Write(px,++py,kvp.Key+" - ",ConsoleColor.White);
                c.Write(kvp.Value.ToString(),ConsoleColor.Gray);
            }
        }
    }
}