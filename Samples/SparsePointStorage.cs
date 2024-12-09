using System;
using System.Collections.Generic;
using NModbus;

namespace Samples
{

    public class SlaveStorage : ISlaveDataStore
    {
        private readonly SparsePointSource<bool> _coils;
        private readonly SparsePointSource<bool> _coilDiscreteInputs;
        private readonly SparsePointSource<ushort> _holdingRegisters;
        private readonly SparsePointSource<ushort> _inputRegisters;

        public SlaveStorage()
        {
            _coils = new SparsePointSource<bool>();
            _coilDiscreteInputs = new SparsePointSource<bool>();
            _holdingRegisters = new SparsePointSource<ushort>();
            _inputRegisters = new SparsePointSource<ushort>();
        }

        //bit Read/Write
        public SparsePointSource<bool> Coils
        {
            get { return _coils; }
        }
        //bit Read only
        public SparsePointSource<bool> CoilDiscreteInputs
        {
            get { return _coilDiscreteInputs; }
        }
        //word Read/Write
        public SparsePointSource<ushort> HoldingRegisters
        {
            get { return _holdingRegisters; }
        }
        //word Read only
        public SparsePointSource<ushort> InputRegisters
        {
            get { return _inputRegisters; }
        }

        IPointSource<bool> ISlaveDataStore.CoilDiscretes
        {
            get { return _coils; }
        }

        IPointSource<bool> ISlaveDataStore.CoilInputs
        {
            get { return _coilDiscreteInputs; }
        }

        IPointSource<ushort> ISlaveDataStore.HoldingRegisters
        {
            get { return _holdingRegisters; }
        }

        IPointSource<ushort> ISlaveDataStore.InputRegisters
        {
            get { return _inputRegisters; }
        }
    }

    /// <summary>
    /// Sparse storage for points.
    /// </summary>
    public class SparsePointSource<TPoint> : IPointSource<TPoint>
    {
        private readonly Dictionary<ushort, TPoint> _values = new Dictionary<ushort, TPoint>();

        public event EventHandler<StorageEventArgs<TPoint>> StorageOperationOccurred;

        /// <summary>
        /// Gets or sets the value of an individual point wih tout 
        /// </summary>
        /// <param name="registerIndex"></param>
        /// <returns></returns>
        public TPoint this[ushort registerIndex]
        {
            get
            {
                TPoint value;

                if (_values.TryGetValue(registerIndex, out value))
                    return value;

                return default(TPoint);
            }
            set { _values[registerIndex] = value; }
        }

        public TPoint[] ReadPoints(ushort startAddress, ushort numberOfPoints)
        {
            var points = new TPoint[numberOfPoints];

            for (ushort index = 0; index < numberOfPoints; index++)
            {
                points[index] = this[(ushort) (index + startAddress)];
            }

            StorageOperationOccurred?.Invoke(this,
                new StorageEventArgs<TPoint>(PointOperation.Read, startAddress, points));

            return points;
        }

        public void WritePoints(ushort startAddress, TPoint[] points)
        {
            for (ushort index = 0; index < points.Length; index++)
            {
                this[(ushort) (index + startAddress)] = points[index];
            }

            StorageOperationOccurred?.Invoke(this,
                new StorageEventArgs<TPoint>(PointOperation.Write, startAddress, points));
        }
    }

    public class StorageEventArgs<TPoint> : EventArgs
    {
        private readonly PointOperation _pointOperation;
        private readonly ushort _startingAddress;
        private readonly TPoint[] _points;

        public StorageEventArgs(PointOperation pointOperation, ushort startingAddress, TPoint[] points)
        {
            _pointOperation = pointOperation;
            _startingAddress = startingAddress;
            _points = points;
        }

        public ushort StartingAddress
        {
            get { return _startingAddress; }
        }

        public TPoint[] Points
        {
            get { return _points; }
        }

        public PointOperation Operation
        {
            get { return _pointOperation; }
        }
    }

    public enum PointOperation
    {
        Read,

        Write
    }


}