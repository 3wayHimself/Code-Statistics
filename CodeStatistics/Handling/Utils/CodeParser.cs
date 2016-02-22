﻿using System;

namespace CodeStatistics.Handling.Utils{
    public class CodeParser{
        public delegate bool IsWhiteSpace(char chr);

        private readonly string code;
        private readonly int length;
        private int cursor;

        public IsWhiteSpace CheckIsWhiteSpace = char.IsWhiteSpace;

        public string Contents { get { return code; } }
        public char Char { get { return cursor < length ? code[cursor] : '\0'; } }
        public bool IsEOF { get { return cursor >= length; } }

        public CodeParser(string code){
            this.code = code;
            this.length = code.Length;
        }

        private CodeParser Clone(string newCode){
            return new CodeParser(newCode){
                CheckIsWhiteSpace = this.CheckIsWhiteSpace
            };
        }

        /// <summary>
        /// Skips to the next character and returns it.
        /// </summary>
        public char Next(){
            ++cursor;
            return Char;
        }

        /// <summary>
        /// Skips all whitespace characters defined by <see cref="CodeParser.CheckIsWhiteSpace"/> and places the cursor after the last found whitespace character.
        /// Returns itself.
        /// </summary>
        public CodeParser SkipSpaces(){
            while(cursor < length){
                if (CheckIsWhiteSpace(Char))++cursor;
                else break;
            }
            
            return this;
        }

        /// <summary>
        /// Skips to the next matching character and returns itself. Caution: The skip may not succeed, so verify if the current character is valid if not certain.
        /// If the skip fails, the cursor will not move.
        /// </summary>
        public CodeParser SkipTo(char chr){
            int prevCursor = cursor;

            while(cursor < length){
                if (Char == chr)break;
                else ++cursor;
            }

            if (Char != chr)cursor = prevCursor;

            return this;
        }

        /// <summary>
        /// Skips a block enclosed by <paramref name="blockStart"/> and <paramref name="blockEnd"/>. If the current character does not match <paramref name="blockStart"/>,
        /// the parser will try skipping to the next <paramref name="blockStart"/> character first. If it fails, the cursor will not move, otherwise the cursor
        /// will be placed after the ending character of the block.
        /// </summary>
        public void SkipBlock(char blockStart, char blockEnd){
            SkipTo(blockStart);
            if (Char != blockStart)return;

            int blockDepth = 0;

            while(cursor < length){
                char next = Next();

                if (next == blockEnd){
                    if (--blockDepth < 0){
                        Next();
                        break;
                    }
                }
                else if (next == blockStart){
                    ++blockDepth;
                }
            }
        }

        /// <summary>
        /// Skips a block enclosed by <paramref name="blockStart"/> and <paramref name="blockEnd"/> and returns a new instance of CodeParser with the contents of that
        /// block, excluding the enclosing characters. If the current character does not match <paramref name="blockStart"/>, the parser will try skipping to the
        /// next <paramref name="blockStart"/>, first. If it fails, the returned contents will be empty and the cursor will not move, otherwise the cursor will be
        /// placed after the ending character of the block.
        /// </summary>
        public CodeParser SkipBlockGet(char blockStart, char blockEnd){
            SkipTo(blockStart);
            if (Char != blockStart)return Clone("");

            int indexStart = cursor;
            SkipBlock(blockStart,blockEnd);

            return Clone(code.Substring(indexStart+1,Math.Min(cursor-indexStart-2,length-1)));
        }
    }
}
