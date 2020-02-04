using System;
using System.Collections.Generic;
using System.Linq;

namespace GitTask
{
    public class Git
    {
        private readonly List<List<(int commitNum, int value)>> _filesHistory;

        private int _lastCommitNum;

        public Git(int filesCount)
        {
            _filesHistory = new List<List<(int commitNum, int value)>>(filesCount);

            for (int i = 0; i < filesCount; i++)
            {
                _filesHistory.Add(new List<(int commitNum, int value)>());
            }
        }

        private static int GetFileStatusAtCommitTime(List<(int commitNum, int value)> fileHistory, ref int commitNumber)
        {
            int begin = 0;
            int end = fileHistory.Count - 1;

            while (begin <= end)
            {
                int mid = (begin + end) / 2;

                if (fileHistory[mid].commitNum > commitNumber)
                {
                    end = mid - 1;
                }
                else
                {
                    if (fileHistory[mid].commitNum == commitNumber ||
                        mid + 1 > end || fileHistory[mid + 1].commitNum > commitNumber)
                    {
                        return fileHistory[mid].value;
                    }

                    begin = mid + 1;
                }
            }

            return 0;
        }

        public void Update(int fileNumber, int value)
        {
            if (!_filesHistory[fileNumber].Any() || _filesHistory[fileNumber].Last().commitNum != _lastCommitNum)
                _filesHistory[fileNumber].Add((_lastCommitNum, value));
            else
                _filesHistory[fileNumber][_filesHistory[fileNumber].Count - 1] = (_lastCommitNum, value);
        }
        public int Commit()
        {
            _lastCommitNum++;
            return _lastCommitNum - 1;
        }
        public int Checkout(int commitNumber, int fileNumber)
        {
            if (_lastCommitNum - 1 < commitNumber)
                throw new ArgumentException("Commit is not found");

            return GetFileStatusAtCommitTime(_filesHistory[fileNumber], ref commitNumber);
        }
    }
}