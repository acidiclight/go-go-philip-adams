using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiColorText
{
    public class TextRenderer
    {
        private Dictionary<char, Color> _colorMap;

        public static TextRenderer Default { get; private set; } = new TextRenderer();

        public Color DefaultColor { get; set; } = Color.White;

        public TextRenderer()
        {
            _colorMap = new Dictionary<char, Color>();
        }

        public Vector2 MeasureString(SpriteFont font, string text, float lineWidth)
        {
            var wrapped = WordWrap(font, text, lineWidth);

            return font.MeasureString(wrapped);
        }

        public Vector2 MeasureString(SpriteFont font, string text)
        {
            return this.MeasureString(font, text, 0);
        }

        public void SetColor(char code, Color color)
        {
            if(_colorMap.ContainsKey(code))
            {
                _colorMap[code] = color;
            }
            else
            {
                _colorMap.Add(code, color);
            }
        }

        public Color GetColor(char code)
        {
            if(_colorMap.ContainsKey(code))
            {
                return _colorMap[code];
            }
            else
            {
                return this.DefaultColor;
            }
        }

        public void RemoveColor(char code)
        {
            if(_colorMap.ContainsKey(code))
            {
                _colorMap.Remove(code);
            }
        }

        public void DrawString(SpriteBatch batch, string text, SpriteFont font, Vector2 position, float lineWidth = 0)
        {
            // First step is to word-wrap the text.
            string wrapped = this.WordWrap(font, text, lineWidth);

            // Now that it's wrapped, we can break the text into sections.
            var breaks = BreakText(wrapped);

            // Track our X position and current line.
            float x = 0;
            int line = 0;
            int lastBreak = -1;
            Color currentColor = DefaultColor;

            // Go through every break.
            foreach(var b in breaks) 
            {
                // Get the text in this break section;
                int pos = lastBreak + 1;
                int len = b.Index - pos;
                string section = wrapped.Substring(pos, len);

                // Render the text.
                batch.DrawString(font, section, new Vector2(position.X + x, position.Y + (line * font.MeasureString(section).Y)), currentColor);

                // Switch to the new color.
                currentColor = b.Color;

                // Switch the break...
                lastBreak = b.Index;

                // If the break was a line break then move to a new line.
                // Otherwise, move forward by physical length of text.
                if(b.IsNewLine)
                {
                    x = 0;
                    line++;
                }
                else
                {
                    x += font.MeasureString(section).X;
                }
                
            }
            
        }

        protected List<TextBreak> BreakText(string text)
        {
            List<TextBreak> breaks = new List<TextBreak>();
            Color currentColor = DefaultColor;
            char lastBreak = '\0';
            bool escaping = false;

            for(int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if(c == '\n')
                {
                    breaks.Add(new TextBreak(i, currentColor, true));
                    lastBreak = c;
                }

                if(c == '\\')
                {
                    if(!escaping)
                    {
                        escaping = true;
                        breaks.Add(new TextBreak(i, currentColor));
                        continue;
                    }
                    else
                    {
                        escaping = false;
                    }
                }

                if(escaping)
                {
                    escaping = false;
                    continue;
                }

                if(_colorMap.ContainsKey(c))
                {
                    if(c == lastBreak)
                    {
                        currentColor = DefaultColor;
                        lastBreak = '\0';
                    }
                    else
                    {
                        currentColor = _colorMap[c];
                        lastBreak = c;
                    }
                    breaks.Add(new TextBreak(i, currentColor));

                }
            }

            breaks.Add(new TextBreak(text.Length, currentColor, false));

            return breaks;
        }

        protected string WordWrap(SpriteFont font, string text, float maxWidth)
        {
            if (maxWidth <= 0) return text;

            // Dear reader,
            //
            // If you've ever played Hacknet and been curious about how Matt did his
            // word-wrapping - instead of just enjoying the gameplay, you're weird.  So am
            // I.  I asked Matt how he did it and he explained the algorithm he settled
            // on.  This is that algorithm, and I'll level with you.  It's a lot better than
            // any word-wrapping code I've written on my own.
            //
            // Matt didn't give me any code, just an idea of what to do, so this is my implementation
            // of the Hacknet word wrapping code.
            //
            // - Michael, or as my English teacher calls me, "Lyfox."

            // Resulting wrapped string...
            StringBuilder sb = new StringBuilder();

            // Current line width.
            float lineWidth = 0;

            // Where are we in the text?
            int textPtr = 0;

            var glyphs = font.GetGlyphs();

            // Keep going till we're out of text.
            while (textPtr < text.Length)
            {
                // Compensation for color codes.  They do not get rendered and do not get measured either.
                float compensation = 0;

                // Escape sequence support.
                bool escaping = false;

                // Current word...
                string word = "";

                // Scan from the beginning of the src string until we
                // hit a space or newline.
                for (int i = textPtr; i < text.Length; i++)
                {
                    char c = text[i];

                    if(c == '\\')
                    {
                        if(!escaping)
                        {
                            compensation += glyphs[c].Width;
                            escaping = true;
                        }
                        else
                        {
                            escaping = false;
                        }
                    }

                    // If the character is a color code then compensate for it.
                    if (_colorMap.ContainsKey(c))
                    {
                        if (escaping)
                        {
                            escaping = false;
                        }
                        else
                        {
                            compensation += glyphs[c].Width;
                        }
                    }

                    // Append the char to the string.
                    word += c;

                    // If the char is a space or newline, end.
                    if (char.IsWhiteSpace(c)) break;
                }

                // Measure the word to get the width.
                float wordWidth = font.MeasureString(word).X - compensation;

                // If the word can't fit on the current line then this is where we wrap.
                if (lineWidth + wordWidth > maxWidth && lineWidth > 0)
                {
                    sb.Append("\r\n");
                    lineWidth = 0;
                }

                // Now, while the word CAN'T fit on its own line, then we'll find a substring that can.
                int wordPtr = 0;
                while (wordWidth > maxWidth)
                {
                    int i = 0;
                    float lw = 0;
                    int p = 0;
                    for (i = wordPtr; i < word.Length; i++)
                    {
                        char c = word[wordPtr + p];
                        if (!_colorMap.ContainsKey(c))
                        {
                            float w = font.MeasureString(c.ToString()).X;
                            if (lw + w > maxWidth)
                            {
                                wordPtr += p;
                                wordWidth -= lw;

                                sb.Append("\r\n");
                                break;
                            }
                            lw += w;
                        }
                        sb.Append(c);
                        p++;
                    }
                }

                // Append the word and increment the line width.
                sb.Append(word.Substring(wordPtr));
                lineWidth += wordWidth;

                // If the word ends with a newline then line width is zero.
                if (word.EndsWith("\n"))
                    lineWidth = 0;

                // Advance the text pointer by the length of the word.
                textPtr += word.Length;
            }

            return sb.ToString();
        }
    }

    public struct TextBreak
    {
        public int Index;
        public Color Color;
        public bool IsNewLine;

        public TextBreak(int index, Color color, bool isNewLine = false)
        {
            IsNewLine = isNewLine;
            Index = index;
            Color = color;
        }
    }
}
