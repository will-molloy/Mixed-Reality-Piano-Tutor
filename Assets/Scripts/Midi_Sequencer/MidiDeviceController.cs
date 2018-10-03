using System.Collections.Generic;
using Sanford.Multimedia.Midi;
using UnityEngine;
using Virtual_Piano;

namespace Midi_Sequencer
{
    /// <summary>
    ///     - Handles MIDI input device
    /// </summary>
    [RequireComponent(typeof(MidiFileSequencer))]
    public sealed class MidiDeviceController : MonoBehaviour
    {
        private InputDevice inputDevice;
        private List<MidiEventStorage> midiEvents;
        private HashSet<PianoKey> notesOn;
        private MidiFileSequencer seq;

        private void Start()
        {
            notesOn = new HashSet<PianoKey>();
            midiEvents = new List<MidiEventStorage>();
            seq = GetComponent<MidiFileSequencer>();

            if (InputDevice.DeviceCount < 1)
            {
                Debug.LogWarning("No device found for MIDI input.");
                return;
            }

            if (InputDevice.DeviceCount > 1)
            {
                Debug.LogWarning("Too many devices found for MIDI input.");
                return;
            }

            inputDevice = new InputDevice(0);
            inputDevice.ChannelMessageReceived += handleChannelMsg;
            inputDevice.ChannelMessageReceived += storeMidiEvent;

            inputDevice.StartRecording();
            Debug.Log("MIDI device inited");
            ClearMidiEventStorage();
        }

        public void ClearMidiEventStorage()
        {
            Debug.Log("Clearing MIDI events storage");
            midiEvents.Clear();
        }

        public List<MidiEventStorage> GetMidiEvents()
        {
            return midiEvents;
        }

        private void OnDestroy()
        {
            Debug.Log("Closing MIDI device.");
            if (inputDevice != null) inputDevice.Dispose();
        }

        private void handleChannelMsg(object sender, ChannelMessageEventArgs e)
        {
            var keyNum = e.Message.Data1;

            if (e.Message.Command == ChannelCommand.NoteOn)
            {
                PianoBuilder.instance.ActivateKey(keyNum, Color.green);

                notesOn.Add(PianoKeys.GetKeyFor(keyNum));
            }
            else if (e.Message.Command == ChannelCommand.NoteOff)
            {
                PianoBuilder.instance.DeactivateKey(keyNum);
                notesOn.Remove(PianoKeys.GetKeyFor(keyNum));
            }
        }

        private void storeMidiEvent(object sender, ChannelMessageEventArgs e)
        {
            if (seq.IsGamedStarted())
                midiEvents.Add(new MidiEventStorage(e, Time.time - seq.GetStartTime()));
        }

        public HashSet<PianoKey> GetOnKeys()
        {
            return notesOn;
        }
    }

    public struct MidiEventStorage
    {
        public float time { get; }

        public int keyNum { get; }

        public bool isEnd { get; }

        public MidiEventStorage(ChannelMessageEventArgs e, float time)
        {
            this.time = time;
            keyNum = e.Message.Data1;
            isEnd = e.Message.Command == ChannelCommand.NoteOff;
        }

        public MidiEventStorage(int keyNum, bool isEnd, float time)
        {
            this.time = time;
            this.keyNum = keyNum;
            this.isEnd = isEnd;
        }
    }
}