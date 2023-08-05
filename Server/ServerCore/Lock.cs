using System;
using System.Threading;

namespace ServerCore
{
    //재귀적 락을 허용할 것인가 (Yes) : WriteLock -> WriteLock(Ok), WriteLock -> ReadLock (OK), ReadLock -> WriteLock (No)
    //(락을 걸었을 때 다른 스레드에서 한번 더 락을 거는것을 허락 할 것인가)
    //스핀락 정책 (5000번 -> Yield)

    public class Lock
    {

        const int EMPTY_FLAG = 0x00000000;
        const int WRITE_MASK = 0x7FFF0000;
        const int READ_MASK = 0x0000FFFF;
        const int MAX_SPIN_COUNT = 5000;

        //[UnUsed(1)]
        //[WriteThreadId(15)] 어느 스레드가 락을 획득 했는지를 구분
        //[ReadCount(16)] 현재 Read Lock 획득한 스레드의 갯수
        int _flag = EMPTY_FLAG;
        int _writeCount = 0;

        public void WriteLock()
        {
            //동일한 스레드가 WriteLock을 획득하고 있는지 확인
            int lockThread = (_flag & WRITE_MASK) >> 16;
            
            if(Thread.CurrentThread.ManagedThreadId == lockThread)
            {
                _writeCount++;
                return;
            }

            //아무도 WriteLock or ReadLock을 획득하고 있지 않을 때, 경합해서 소유권을 얻는다.
            //thread id의 비트를 획득
            int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;
            
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    //성공 시 return
                    if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                    {
                        _writeCount = 1;
                        return;
                    }
                }

                Thread.Yield();
            }
        }

        public void WriteUnlock()
        {
            int lockCount = --_writeCount;

            if(lockCount == 0)
                Interlocked.Exchange(ref _flag, EMPTY_FLAG);
        }

        public void ReadLock()
        {
            //동일한 스레드가 WriteLock을 획득하고 있는지 확인
            int lockThread = (_flag & WRITE_MASK) >> 16;

            if (Thread.CurrentThread.ManagedThreadId == lockThread)
            {
                Interlocked.Increment(ref _flag);
                return;
            }

            //아무도 WriteLock을 획득하고 있지 않으면, ReadCount를 1 늘린다.
            while (true)
            {
                for (int i = 0; i < MAX_SPIN_COUNT; i++)
                {
                    int expected = (_flag & READ_MASK);
                    if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                        return;
                }

                Thread.Yield();
            }
        }

        public void ReadUnlock()
        {
            Interlocked.Decrement(ref _flag);
        }
    }
}
