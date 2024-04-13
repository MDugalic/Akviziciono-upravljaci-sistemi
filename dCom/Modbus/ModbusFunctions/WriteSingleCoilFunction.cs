using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write coil functions/requests.
    /// </summary>
    public class WriteSingleCoilFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleCoilFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleCoilFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] req = new byte[12];

            ushort transactionId = CommandParameters.TransactionId;
            ushort protocolId = CommandParameters.ProtocolId;
            ushort length = CommandParameters.Length;
            byte unitId = CommandParameters.UnitId;
            byte functionCode = CommandParameters.FunctionCode;

            ushort outputAddress = ((ModbusWriteCommandParameters)CommandParameters).OutputAddress;
            ushort value = ((ModbusWriteCommandParameters)CommandParameters).Value;

            // Postavljanje TransactionId (2 bajta)
            req[0] = (byte)(transactionId >> 8); // Viši bajt
            req[1] = (byte)transactionId;        // Niži bajt

            // Postavljanje ProtocolId (2 bajta)
            req[2] = (byte)(protocolId >> 8);   // Viši bajt
            req[3] = (byte)protocolId;          // Niži bajt

            // Postavljanje Length (2 bajta)
            req[4] = (byte)(length >> 8);   // Viši bajt
            req[5] = (byte)length;          // Niži bajt

            // Postavljanje UnitId (1 bajt)
            req[6] = unitId;

            // Postavljanje FunctionCode (1 bajt)
            req[7] = functionCode;

            // Postavljanje OutputAddress (2 bajta)
            req[8] = (byte)(outputAddress >> 8);  // Viši bajt
            req[9] = (byte)outputAddress;         // Niži bajt

            // Postavljanje Value (2 bajta)
            req[10] = (byte)(value >> 8);  // Viši bajt
            req[11] = (byte)value;         // Niži bajt

            return req;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> resp = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort outputAddress = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 8));
            ushort value = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 10));

            Tuple<PointType, ushort> tuple = new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, outputAddress);
            resp.Add(tuple, value);

            return resp;
        }
    }
}