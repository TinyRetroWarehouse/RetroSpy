﻿namespace RetroSpy.Readers
{
    internal class Pippin
    {
        private const int PACKET_SIZE = 62;
        private const int POLISHED_PACKET_SIZE = 32;

        private static readonly string[] BUTTONS = {
            null, "1", "2", "blue", "yellow", "up", "left", "right", "down", "red", "green", "square", "circle", "diamond"
        };

        private static readonly string[] KEYBOARD_CODES =
        {
            "A", "S", "D", "F", "H", "G", "Z", "X",
            "C", "V", null, "B", "Q", "W", "E", "R",
            "Y", "T", "D1", "D2", "D3", "D4", "D6", "D5",
            "Equals", "D9", "D7", "Minus", "D8", "D0", "RightBracket", "O",

            "U", "LeftBracket", "I", "P", "Return", "L", "J", "Apostrophe",
            "K", "Semicolon", "Backslash", "Comma", "Slash", "N", "M", "Period",
            "Tab", "Space", "Grave", "Back", null, "Escape", "LeftControl", "LeftAlt",
            "LeftShift", "Capital", "LeftWindowsKey", "Left", "Right", "Down", "Up", null,

            null, "Decimal", null, "Subtract" /*Multiply*/, null, "MacAdd" /*Add*/, null, "NumberLock" /*Clear*/,
            null, null, null, "Multiply", "NumberPadEnter", null, "Add" /* Subtract */, null,
            null, "Divide" /* NumPad Equal */, "NumberPad0", "NumberPad1", "NumberPad2", "NumberPad3", "NumberPad4", "NumberPad5",
            "NumberPad6", "NumberPad7", null, "NumberPad8", "NumberPad9", null, null, null,

            "F5", "F6", "F7", "F3", "F8", "F9", null, "F11",
            null, "PrintScreen", null, "ScrollLock", null, "F10", null, "F12",
            null, "Pause", "Insert", "Home", "PageUp", "Delete", "F4", "End",
            "F2", "PageDown", "F1", null, null, null, null, "Power",
        };

        private static float ReadMouse(byte data)
        {
            if (data >= 64)
            {
                return (-1.0f * (128 - data)) / 64.0f;
            }
            else
            {
                return data / 64.0f;
            }
        }

        public static ControllerStateEventArgs ReadFromPacket(byte[] packet)
        {
            if (packet.Length == PACKET_SIZE)
            {

                byte[] polishedPacket = new byte[POLISHED_PACKET_SIZE];

                polishedPacket[0] = (byte)((packet[0] >> 4) | packet[1]);
                polishedPacket[1] = (byte)(packet[9] == 0 ? 0 : 1);
                polishedPacket[2] = (byte)(packet[17] == 0 ? 0 : 1);

                for (int i = 18; i < 29; ++i)
                {
                    polishedPacket[i - 15] = (byte)(packet[i] == 0 ? 0 : 1);
                }

                for (byte i = 0; i < 7; ++i)
                {
                    polishedPacket[14] |= (byte)((packet[i + 2] == 0 ? 0 : 1) << i);
                }

                for (byte i = 0; i < 7; ++i)
                {
                    polishedPacket[15] |= (byte)((packet[i + 10] == 0 ? 0 : 1) << i);
                }

                for (int i = 30; i < 62; i += 2)
                {
                    polishedPacket[(i / 2) + 1] = (byte)((packet[i] >> 4) | packet[i + 1]);
                }

                ControllerStateBuilder state = new ControllerStateBuilder();

                for (int i = 0; i < BUTTONS.Length; ++i)
                {
                    if (string.IsNullOrEmpty(BUTTONS[i]))
                    {
                        continue;
                    }

                    state.SetButton(BUTTONS[i], polishedPacket[i] == 0x00);
                }

                float x = 0;
                float y = 0;
                if (packet[29] == 1) // This is the "Has Data" bit.  Reset the mouse on cached results.
                {
                    y = ReadMouse(polishedPacket[14]);
                    x = ReadMouse(polishedPacket[15]);
                }
                SignalTool.SetMouseProperties(x, y, state);

                for (int i = 0; i < KEYBOARD_CODES.Length; ++i)
                {
                    if (KEYBOARD_CODES[i] != null)
                    {
                        state.SetButton(KEYBOARD_CODES[i], (polishedPacket[(i / 8) + 16] & (1 << (i % 8))) != 0);
                    }
                }

                return state.Build();
            }
            else
                return null;
        }
    }
}