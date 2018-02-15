using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace SynthFW
{
    public abstract class Signal<T>
    {
        public abstract int Channels { get; }
        public abstract int BlockSize { get; }

        public abstract void NextBlock(byte blockNr);
        public abstract T this[int sampleNr, int channel] { get; }
        public Signal<R> Flat<R>(Func<R,T,R> aggregator, R seed) => new FlattenedSignal<R>(this, aggregator, seed);

        private class FlattenedSignal<R> : Signal<R>
        {
            private Signal<T> _signal;
            private Func<R,T,R> _aggregator;
            private R _seed;
            public FlattenedSignal(Signal<T> signal, Func<R,T,R> aggregator, R seed)
            {
                _signal = signal;
                _aggregator = aggregator;
                _seed = seed;
            }

            public override R this[int sampleNr, int channel] => Enumerable.Range(0, _signal.Channels)
                .Select(i =>_signal[sampleNr, i])
                .Aggregate(_seed,_aggregator);

            public override int Channels => 1;

            public override int BlockSize => _signal.BlockSize;

            public override void NextBlock(byte blockNr)
            {
                _signal.NextBlock(blockNr);
            }
        }
    }

    public interface ISignalSource<T>
    {
       void GetBlock(T[,] buffer, byte blockNr);
    }

    public class DynamicSignal<T> : Signal<T>
    {
        public string Name;
        private ISignalSource<T> _source;
        protected byte _blockNr;
        public T[,] Buffer;

        public override int Channels => Buffer.GetLength(1);

        public override int BlockSize => Buffer.GetLength(0);

        public override T this[int sampleNr, int channel] => Buffer[sampleNr % BlockSize, channel % Channels];

        public override void NextBlock(byte blockNr)
        {
            if (_blockNr != blockNr)
            {
                if (Buffer == null)
                    throw new SlooException(Name, "null output buffer");
                if (_source == null)
                    throw new SlooException(Name, "null source");
                _blockNr = blockNr;
                try
                {
                    _source.GetBlock(Buffer, blockNr);
                }
                catch(Exception e) when (!(e is SlooException))
                {
                    throw new SlooException(Name, "failed to receive a block", e);
                }
            }
        }

        public DynamicSignal(ISignalSource<T> source, string name)
        {
            Name = name;
            _source = source;
        }
    }

    public class ConstantSignal<T> : Signal<T>
    {
        private Func<T> _source;
        private T _val;

        public override int Channels => 1;

        public override int BlockSize => 1;

        public override T this[int sampleNr, int channel] => _val;

        public ConstantSignal(Func<T> src)
        {
            _source = src;
            _val = _source();
        }

        public override void NextBlock(byte blockNr)
        {
            _val = _source();
        }
    }

    public class MultiConstantSignal<T> : Signal<T>
    {
        private Func<T[]> _source;
        private T[] _val;

        public override int Channels => _val.Length;

        public override int BlockSize => 1;

        public override T this[int sampleNr, int channel] => _val[channel % Channels];

        public MultiConstantSignal(Func<T[]> src)
        {
            _source = src;
            _val = _source();
        }

        public override void NextBlock(byte blockNr)
        {
            _val = _source();
        }
    }


}
