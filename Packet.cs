using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace highprefnet1._0
{
    public class Packet
    {
        int packetId;
        int packetSize;
        int input_time;
        int output_time;
        Double byte_marker;

        public Packet(int id, int totalBytes)
        {
            packetId = id;
            packetSize = totalBytes;
            input_time = 0;
            output_time = 0;
            byte_marker = 0;
        }
      
        public int PacketId
        {
            get
            {
                return this.packetId;
            }
            set
            {
                this.packetId = value;
            }
        }

        public int PacketSize
        {
            get
            {
                return this.packetSize;
            }
            set
            {
                this.packetSize = value;
            }
        }
        public int Input_time
        {
            get
            {
                return this.input_time;
            }
            set
            {
                this.input_time = value;
            }
        }
        public int Output_time
        {
            get
            {
                return this.output_time;
            }
            set
            {
                this.output_time = value;
            }
        }
        public Double Byte_marker
        {
            get
            {
                return this.byte_marker;
            }
            set
            {
                this.byte_marker = value;
            }
        }
    }
}
