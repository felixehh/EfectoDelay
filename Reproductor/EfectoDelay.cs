using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NAudio.Wave;

namespace Reproductor
{
    class EfectoDelay : ISampleProvider
    {
        private ISampleProvider fuente;

        public int offsetMs;
        private List<float> muestras = new List<float>();
        private int tamañoBuffer;

        private int cantidadMuestrasTranscurridas = 0;
        private int cantidadMuestrasBorradas = 0;

        public EfectoDelay(ISampleProvider fuente, int offsetMs)
        {
            this.fuente = fuente;
            this.offsetMs = offsetMs;

            tamañoBuffer = fuente.WaveFormat.SampleRate * 20 * fuente.WaveFormat.Channels; 
        }

        public WaveFormat WaveFormat
        {
            get
            {
                return fuente.WaveFormat;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = fuente.Read(buffer, offset, count);
            //calculo de tiempo
            float milisegundosTranscurridos = ((float)(cantidadMuestrasTranscurridas) / (float)(fuente.WaveFormat.SampleRate * fuente.WaveFormat.Channels)) * 1000.0f;
            int numeroMuestrasOffset = (int) ((offsetMs / 1000.0f) * (float) fuente.WaveFormat.SampleRate);

            //llenar el buffer
            for (int i=0; i<read; i++)
            {
                muestras.Add(buffer[i + offset]);
            }

            //si el buffer excede el tamaño, ajustar el num de elementos
            if(muestras.Count > tamañoBuffer)
            {
                //eliminar excedente
                int diferencia = muestras.Count - tamañoBuffer;
                muestras.RemoveRange(0, diferencia);
                cantidadMuestrasBorradas += diferencia;
            }
            //aplicar el efecto
            if (milisegundosTranscurridos > offsetMs)
            {
                for(int i=0; i<read; i++)
                {
                    buffer[i + offset] += muestras[(cantidadMuestrasTranscurridas - cantidadMuestrasBorradas) + i];
                }
            }

            cantidadMuestrasTranscurridas += read;

            return read;
        }
    }
}
