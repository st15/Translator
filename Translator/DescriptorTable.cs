using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Translator
{
    class DescriptorTable
    {
        private Hashtable ht = new Hashtable();
        private int nextvalue = 0; //����� � ������� ������ bp �� �����

        // ���������� ���������� � ��������� �� �������������
        // ��� ���� ���������� � ���������, ����� ������������ � ����� � �������
        public int Register(string str)
        {
            if (ht.ContainsKey(str))
            {
                return (int)ht[str];
            }
            else
            {
                nextvalue = nextvalue + 4;
                ht.Add(str, nextvalue);

                return nextvalue;
            }

        }

    }
}
