﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using NModbus.Data;
using NModbus.Interfaces;
using NModbus.IO;
using NModbus.Message;
using NModbus.Device;

namespace NModbus.Device
{
    /// <summary>
    ///     Modbus serial master device.
    /// </summary>
    internal class ModbusSerialMaster : ModbusMaster, IModbusSerialMaster
    {
        internal ModbusSerialMaster(IModbusSerialTransport transport)
            : base(transport)
        {
        }

        /// <summary>
        ///     Gets the Modbus Transport.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IModbusSerialTransport IModbusSerialMaster.Transport
        {
            get { return (IModbusSerialTransport)Transport; }
        }

        ///// <summary>
        /////     Modbus ASCII master factory method.
        ///// </summary>
        //public static ModbusSerialMaster CreateAscii(TcpClient tcpClient)
        //{
        //    if (tcpClient == null)
        //    {
        //        throw new ArgumentNullException(nameof(tcpClient));
        //    }

        //    return CreateAscii(new TcpClientAdapter(tcpClient));
        //}

        ///// <summary>
        /////     Modbus ASCII master factory method.
        ///// </summary>
        //public static ModbusSerialMaster CreateAscii(UdpClient udpClient)
        //{
        //    if (udpClient == null)
        //    {
        //        throw new ArgumentNullException(nameof(udpClient));
        //    }

        //    if (!udpClient.Client.Connected)
        //    {
        //        throw new InvalidOperationException(Resources.UdpClientNotConnected);
        //    }

        //    return CreateAscii(new UdpClientAdapter(udpClient));
        //}

        ///// <summary>
        /////     Modbus ASCII master factory method.
        ///// </summary>
        //public static ModbusSerialMaster CreateAscii(IStreamResource streamResource)
        //{
        //    if (streamResource == null)
        //    {
        //        throw new ArgumentNullException(nameof(streamResource));
        //    }

        //    return new ModbusSerialMaster(new ModbusAsciiTransport(streamResource));
        //}

        ///// <summary>
        /////     Modbus RTU master factory method.
        ///// </summary>
        //public static ModbusSerialMaster CreateRtu(TcpClient tcpClient)
        //{
        //    if (tcpClient == null)
        //    {
        //        throw new ArgumentNullException(nameof(tcpClient));
        //    }

        //    return CreateRtu(new TcpClientAdapter(tcpClient));
        //}

        ///// <summary>
        /////     Modbus RTU master factory method.
        ///// </summary>
        //public static ModbusSerialMaster CreateRtu(UdpClient udpClient)
        //{
        //    if (udpClient == null)
        //    {
        //        throw new ArgumentNullException(nameof(udpClient));
        //    }

        //    if (!udpClient.Client.Connected)
        //    {
        //        throw new InvalidOperationException(Resources.UdpClientNotConnected);
        //    }

        //    return CreateRtu(new UdpClientAdapter(udpClient));
        //}

        ///// <summary>
        /////     Modbus RTU master factory method.
        ///// </summary>
        //public static ModbusSerialMaster CreateRtu(IStreamResource streamResource)
        //{
        //    //if (streamResource == null)
        //    //{
        //    //    throw new ArgumentNullException(nameof(streamResource));
        //    //}

        //    //return new ModbusSerialMaster(new ModbusRtuTransport(streamResource));

        //    throw new NotImplementedException();
        //}

        /// <summary>
        ///     Serial Line only.
        ///     Diagnostic function which loops back the original data.
        ///     NModbus only supports looping back one ushort value, this is a limitation of the "Best Effort" implementation of
        ///     the RTU protocol.
        /// </summary>
        /// <param name="slaveAddress">Address of device to test.</param>
        /// <param name="data">Data to return.</param>
        /// <returns>Return true if slave device echoed data.</returns>
        public bool ReturnQueryData(byte slaveAddress, ushort data)
        {
            DiagnosticsRequestResponse request = new DiagnosticsRequestResponse(
                ModbusFunctionCodes.DiagnosticsReturnQueryData,
                slaveAddress,
                new RegisterCollection(data));

            DiagnosticsRequestResponse response = Transport.UnicastMessage<DiagnosticsRequestResponse>(request);

            return response.Data[0] == data;
        }
    }
}
