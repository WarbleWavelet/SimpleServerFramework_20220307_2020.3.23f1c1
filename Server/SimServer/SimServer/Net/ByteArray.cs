using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimServer.Net
{
    /// <summary>处理数据</summary>
    public class ByteArray
    {

        #region 字段属性构造
        #region 字段
        /// <summary>默认大小</summary>
        public const int DEFAULT_SIZE = 1024;
        /// <summary>初始大小</summary>
        private int m_InitSize = 0;
        /// <summary>缓冲区</summary>
        public byte[] Bytes;
        /// <summary>读写位置, ReadIdx = 开始读的索引，WriteIdx = 已经写入的索引</summary>
        public int ReadIdx = 0;
        /// <summary>已经写入的索引</summary>
        public int WriteIdx = 0;
        /// <summary>容量</summary>
        private int Capacity = 0;
        #endregion

        #region 属性
        /// <summary>剩余空间</summary>
        public int Remain { get { return Capacity - WriteIdx; } }
        /// <summary>数据长度</summary>
        public int Length { get { return WriteIdx - ReadIdx; } }

        #endregion

        #region 构造
        public ByteArray()
        {
            Bytes = new byte[DEFAULT_SIZE];
            Capacity = DEFAULT_SIZE;
            m_InitSize = DEFAULT_SIZE;
            ReadIdx = 0;
            WriteIdx = 0;
        }
        #endregion
        #endregion



        #region 辅助1
        /// <summary> 检测并移动数据 </summary>
        public void CheckAndMoveBytes()
        {
            if (Length < 8)
            {
                MoveBytes();
            }
        }

        /// <summary> 移动数据 </summary>
        public void MoveBytes()
        {
            if (ReadIdx < 0)
                return;

            Array.Copy(Bytes, ReadIdx, Bytes, 0, Length);
            WriteIdx = Length;
            ReadIdx = 0;
        }

        /// <summary>
        /// 扩充
        /// </summary>
        /// <param name="size"></param>
        public void ReSize(int size)
        {
            if (ReadIdx < 0) return;
            if (size < Length) return;
            if (size < m_InitSize) return;
            int n = 1024;

            while (n < size) 
                n *= 2;

            Capacity = n;
            byte[] newBytes = new byte[Capacity];
            Array.Copy(Bytes, ReadIdx, newBytes, 0, Length);
            Bytes = newBytes;
            WriteIdx = Length;
            ReadIdx = 0;
        }
        #endregion

    }
}
