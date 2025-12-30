
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ExitGames.Client.Photon;
namespace Core.Model
{
    public class Character
    {
        public string nickname;
        public string color;
        public int score;
        public float currentHealth;
        public int maxHealth;
        public bool isDead;

        public Character() { }

        public Character(string nickname, string color, int maxHealth)
        {
            this.score = 0;
            this.nickname = nickname;
            this.color = color;
            this.maxHealth = maxHealth;
            this.currentHealth = this.maxHealth;
            this.isDead = false;
        }

        public int increaseScoreByValue(int amount)
        {
            score = score + amount;
            return score;
        }
        public bool decreaseHealth(int amount)
        {   

            currentHealth = currentHealth - amount;
            if (currentHealth <= 0)
            {
                isDead = true;
            }
            return isDead;
        }

        public static byte[] Serialize(object obj)
        {
            Character c = (Character) obj;
            byte[] nicknameBytes = Encoding.UTF8.GetBytes(c.nickname);
            byte[] colorBytes = Encoding.UTF8.GetBytes(c.color);

            int size =
                1 + nicknameBytes.Length +
                1 + colorBytes.Length +
                4 + // score
                4 + // currentHealth
                4 + // maxHealth
                1;  // isDead

            byte[] buffer = new byte[size];
            int index = 0;

            // nickname
            buffer[index++] = (byte)nicknameBytes.Length;
            Array.Copy(nicknameBytes, 0, buffer, index, nicknameBytes.Length);
            index += nicknameBytes.Length;

            // color
            buffer[index++] = (byte)colorBytes.Length;
            Array.Copy(colorBytes, 0, buffer, index, colorBytes.Length);
            index += colorBytes.Length;

            // score
            WriteShort(buffer, ref index, c.score);

            // currentHealth
            WriteFloat(buffer, ref index, c.currentHealth);

            // maxHealth
            WriteShort(buffer, ref index, c.maxHealth);

            // isDead
            buffer[index++] = (byte)(c.isDead ? 1 : 0);

            return buffer;
        }


        public static object Deserialize(byte[] data)
        {
            Character c = new Character();
            int index = 0;

            // nickname
            int nicknameLength = data[index++];
            c.nickname = Encoding.UTF8.GetString(data, index, nicknameLength);
            index += nicknameLength;

            // color
            int colorLength = data[index++];
            c.color = Encoding.UTF8.GetString(data, index, colorLength);
            index += colorLength;

            // score
            c.score = ReadShort(data, ref index);

            // currentHealth
            c.currentHealth = ReadFloat(data, ref index);

            // maxHealth
            c.maxHealth = ReadShort(data, ref index);

            // isDead
            c.isDead = data[index++] == 1;

            return c;
        }

        private static void WriteShort(byte[] buffer, ref int index, int value)
        {
            buffer[index++] = (byte)(value >> 8);
            buffer[index++] = (byte)(value);
        }

        private static short ReadShort(byte[] buffer, ref int index)
        {
            return (short)((buffer[index++] << 8) | buffer[index++]);
        }

        private static void WriteFloat(byte[] buffer, ref int index, float value)
        {
            int intValue = BitConverter.SingleToInt32Bits(value);

            buffer[index++] = (byte)(intValue >> 24);
            buffer[index++] = (byte)(intValue >> 16);
            buffer[index++] = (byte)(intValue >> 8);
            buffer[index++] = (byte)(intValue);
        }

        private static float ReadFloat(byte[] buffer, ref int index)
        {
            int value =
                (buffer[index++] << 24) |
                (buffer[index++] << 16) |
                (buffer[index++] << 8) |
                buffer[index++];

            return BitConverter.Int32BitsToSingle(value);
        }
    }


}


