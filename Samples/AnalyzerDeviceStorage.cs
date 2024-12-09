using NModbus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Samples
{
    public class AnalyzerDeviceStorage : ISlaveDataStore
    {
        public int CoilsWordSize { get; set; } = 0;
        public int DiscreteInputWordSize { get; set; } = 3; //Word X 3. => 16 * 3
        public int HoldingRegisterWordSize { get; set; } = 0;
        public int InputRegisterWordSize { get; set; } = 24;
        public int StartMemoryIndex { get; set; } = 0;

        private CancellationTokenSource cts_ = new CancellationTokenSource();

        public IPointSource<bool> CoilDiscretes
        {
            get
            {
                return coils_;
            }
        }
        public IPointSource<bool> CoilInputs
        {
            get
            {
                return coilDiscreteInputs_;
            }
        }

        public IPointSource<ushort> HoldingRegisters
        {
            get
            {
                return holdingRegisters_;
            }
        }

        public IPointSource<ushort> InputRegisters
        {
            get
            {
                return inputRegisters_;
            }
        }

        //private BitArray coils_;
        //private BitArray discreteInput_;
        //private List<ushort> holingRegister_;
        //private List<ushort> inputRegister_;
        //read-write. Bit
        private SparsePointSource<bool> coils_;
        //read only. Bit
        private SparsePointSource<bool> coilDiscreteInputs_;
        //read-write. 16bits word
        private SparsePointSource<ushort> holdingRegisters_;
        //read only. 16bits word
        private SparsePointSource<ushort> inputRegisters_;

        public AnalyzerDeviceStorage()
        {
            ReallocateMemory();
            StartHeartbitSignal(cts_.Token);
        }

        public void ReallocateMemory()
        {
            coils_ = new SparsePointSource<bool>();
            for (int i = 0; i < CoilsWordSize * 16; i++)
            {
                coils_[(ushort)i] = false;
            }
            coilDiscreteInputs_ = new SparsePointSource<bool>();
            for (int i = 0; i < DiscreteInputWordSize * 16; i++)
            {
                coils_[(ushort)i] = false;
            }
            holdingRegisters_ = new SparsePointSource<ushort>();
            for (int i = 0; i < HoldingRegisterWordSize; i++)
            {
                holdingRegisters_[(ushort)i] = 0;
            }
            inputRegisters_ = new SparsePointSource<ushort>();
            for (int i = 0; i < InputRegisterWordSize; i++)
            {
                inputRegisters_[(ushort)i] = 0;
            }
        }
        public void SetCoil(ushort index, bool  value)
        {
            coils_[index] = value;
        }
        public bool GetCoil(ushort index)
        {
            return coils_[index];
        }

        public void SetDiscreteInput(ushort index, bool value)
        {
            coilDiscreteInputs_[index] = value;
        }
        public bool GetDiscreteInput(ushort index)
        {
            return coilDiscreteInputs_[index];
        }   
        public bool SetDiscreteInputs(bool standbyMode, bool analysisMode, bool controlMode, bool remoteMode, bool runwaitState,
            bool pump1On, bool pump2On, bool pump3On, bool pump4On, bool pump5On, bool pump6On, bool pump7On, bool pump8On)
        {
            SetDiscreteInput(1, standbyMode);
            SetDiscreteInput(2, analysisMode);
            SetDiscreteInput(3, controlMode);
            SetDiscreteInput(4, remoteMode);
            SetDiscreteInput(5, runwaitState);
            SetDiscreteInput(16, pump1On);
            SetDiscreteInput(17, pump2On);
            SetDiscreteInput(18, pump3On);
            SetDiscreteInput(19, pump4On);
            SetDiscreteInput(20, pump5On);
            SetDiscreteInput(21, pump6On);
            SetDiscreteInput(22, pump7On);
            SetDiscreteInput(23, pump8On);
            return true;
        }
        public void SetHoldingRegister(ushort index, ushort value)
        {
            holdingRegisters_[index] = value;
        }
        public ushort GetHoldingRegister(ushort index)
        {
            return holdingRegisters_[index];
        }
        public void SetInputRegister(ushort index, byte low, byte high)
        {
            inputRegisters_[index] = (ushort)(high << 8 | low);
        }
        public void SetInputRegister(ushort index, char low, char high)
        {
            inputRegisters_[index] = (ushort)(high << 8 | low);
        }
        public void SetInputRegister(ushort index, ushort value)
        {
            inputRegisters_[index] = value;
        }
        public void SetInputRegister(ushort index, float value)
        {
            byte[] array = System.BitConverter.GetBytes(value);
            inputRegisters_[index] = (ushort)(array[0] | array[1] << 8);
            inputRegisters_[(ushort)(index+1)] = (ushort)(array[2] | array[3] << 8);
        }

        public ushort GetInputRegister(ushort index)
        {
            return inputRegisters_[index];
        }
        public void HeartBitTask()
        {
            if ( false == coilDiscreteInputs_[0])
            {
                SetDiscreteInput(0, true);            }
            else
            {
                SetDiscreteInput(0, false);
            }
        }
        public async void StartHeartbitSignal(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Run(() => HeartBitTask());
                }
                catch (Exception)
                {
                }
                await Task.Delay(1000);
            }
        }
        public bool SetHoldingRegister(byte deviceAddress, char deviceStatus, byte serialNumber, byte tankNo, 
            DateTime samplingTime, float fA, float fB, float fC, float fD, float fE, 
            float fRepA, float fRepB, float fRepC, float fRepD, float fRepE,
            char evalA, char evalB, char evalC, char evalD, char evalE, char alarmCount, ushort acode1, ushort acode2, ushort acode3)
        {
            SetInputRegister(0, deviceAddress, (byte)deviceStatus);
            SetInputRegister(1, serialNumber, tankNo);
            SetInputRegister(2, IntToBCD4(samplingTime.Year));
            SetInputRegister(3, IntToBCD2(samplingTime.Month), IntToBCD2(samplingTime.Day));
            SetInputRegister(4, IntToBCD2(samplingTime.Hour), IntToBCD2(samplingTime.Minute));
            SetInputRegister(5, IntToBCD2(samplingTime.Second), (byte)0);
            SetInputRegister(6, fA); SetInputRegister(8, fB); SetInputRegister(10, fC); SetInputRegister(12, fD); SetInputRegister(14, fE); 
            SetInputRegister(16, fRepA); SetInputRegister(18, fRepB); SetInputRegister(20, fRepC); SetInputRegister(22, fRepD); SetInputRegister(24, fRepE); 
            SetInputRegister(26, evalA, evalB);
            SetInputRegister(27, evalC, evalD);
            SetInputRegister(28, (char) 0, (char) 0);
            SetInputRegister(29, alarmCount, (char)0);
            SetInputRegister(30, acode1);
            SetInputRegister(31, acode2);
            SetInputRegister(32, acode3);
            return true;
        }
        static byte[] IntToBCD(int input)
        {
            if (input > 9999 || input < 0)
                throw new ArgumentOutOfRangeException("input");

            int thousands = input / 1000;
            int hundreds = (input -= thousands * 1000) / 100;
            int tens = (input -= hundreds * 100) / 10;
            int ones = (input -= tens * 10);

            byte[] bcd = new byte[] {
                (byte)(thousands << 4 | hundreds),
                (byte)(tens << 4 | ones)
            };
            return bcd;
        }
        static ushort IntToBCD4(int input)
        {
            if (input > 9999 || input < 0)
                throw new ArgumentOutOfRangeException("input");

            int thousands = input / 1000;
            int hundreds = (input -= thousands * 1000) / 100;
            int tens = (input -= hundreds * 100) / 10;
            int ones = (input -= tens * 10);

            ushort bcd = (ushort)(ones << 12 | tens << 8 | hundreds << 4 | thousands);
            return bcd;
        }
        static byte IntToBCD2(int input)
        {
            if (input > 99 || input < 0)
                throw new ArgumentOutOfRangeException("input");

            int tens = input / 10;
            int ones = (input -= tens * 10);

            byte bcd = (byte)(ones << 4 | tens);
            return bcd;
        }

    }
}
