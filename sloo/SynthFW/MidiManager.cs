using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynthFW
{
    public class MidiManager
    {
        public int NoteChannels { get; private set; }
        
        public MultiConstantSignal<bool> ChannelTriggers { get; private set; }
        public MultiConstantSignal<double> ChannelFrequencies { get; private set; }


        private static Note[] _notes;
        public static Note[] Notes
        {
            get
            {
                if (_notes == null)
                {
                    _notes = new Note[255];
                    for (byte i = 0; i < 255; i++)
                    {
                        _notes[i] = new Note(i);
                    }
                }
                return _notes;
            }
        }

        private Dictionary<byte, PlayedNote> _playedNotes = new Dictionary<byte, PlayedNote>();
        private Dictionary<byte, int> _noteChannelMapping = new Dictionary<byte, int>();
        private Dictionary<int, PlayedNote> _channelNoteMapping = new Dictionary<int, PlayedNote>();
        private Queue<int> _freeChannels = new Queue<int>();

        public MidiManager(int channels)
        {
            NoteChannels = channels;

            ChannelFrequencies = new MultiConstantSignal<double>(InputFrequencies);
            ChannelTriggers = new MultiConstantSignal<bool>(InputTriggers);
            Enumerable.Range(0, NoteChannels).ForEach(c => _freeChannels.Enqueue(c));
        }


        public void NoteOn(byte idx)
        {
            _playedNotes[idx] = new PlayedNote()
            {
                Note = Notes[idx],
            };
            if (_freeChannels.Count() == 0)
                return;
            var newChannel = _freeChannels.Dequeue();
            _noteChannelMapping[idx] = newChannel;
            _channelNoteMapping[newChannel] = _playedNotes[idx];
        }

        public void NoteOff(byte idx)
        {
            _playedNotes.Remove(idx);
            if (!(_noteChannelMapping.TryGetValue(idx, out var channel)))
                return;
            _noteChannelMapping.Remove(idx);
            _channelNoteMapping.Remove(channel);
            _freeChannels.Enqueue(channel);
        }


        private double[] InputFrequencies() =>
           Enumerable.Range(0, NoteChannels)
               .Select(channel =>
                   (_channelNoteMapping.TryGetValue(channel, out var note)
                       ? note.Note.Frequency
                       : 0)
               ).ToArray();


        private bool[] InputTriggers() =>
            Enumerable.Range(0, NoteChannels)
                .Select(channel =>
                    (_channelNoteMapping.TryGetValue(channel, out var note)
                        ? true
                        : false)
                ).ToArray();

        public struct Note
        {
            public byte Idx { get; private set; }
            public double Frequency { get; private set; }
            public Note(byte idx)
            {
                Idx = idx;
                Frequency = Utils.MidiToFrequency(idx);
            }
        }

        private class PlayedNote
        {
            public Note Note;
        }
    }
}
