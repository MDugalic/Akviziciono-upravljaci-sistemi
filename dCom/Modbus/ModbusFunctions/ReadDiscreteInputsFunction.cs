using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
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

            ushort startAddress = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
            ushort quantity = ((ModbusReadCommandParameters)CommandParameters).Quantity;

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
            req[8] = (byte)(startAddress >> 8);  // Viši bajt
            req[9] = (byte)startAddress;         // Niži bajt

            // Postavljanje Value (2 bajta)
            req[10] = (byte)(quantity >> 8);  // Viši bajt
            req[11] = (byte)quantity;         // Niži bajt

            return req;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> resp = new Dictionary<Tuple<PointType, ushort>, ushort>();

            int byteCount = response[8];
            ushort startAddress = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
            ushort counter = 0;

            for (int i = 0; i < byteCount; i++)
            {
                byte temp = response[9 + i];
                byte mask = 1;

                ushort quantity = ((ModbusReadCommandParameters)CommandParameters).Quantity;

                for (int j = 0; j < 8; j++)
                {
                    ushort value = (ushort)(temp & mask);
                    Tuple<PointType, ushort> tuple = new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, startAddress++);
                    resp.Add(tuple, value);

                    temp >>= 1;
                    counter++;

                    if (counter >= quantity)
                        break;
                }
            }

            return resp;
        }
    }
}