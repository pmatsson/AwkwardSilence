using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore.CoreAudioAPI;
using System.Diagnostics;

namespace NoAwkwardSilence
{
    public struct AudioSession
    {
        public string name { get; private set; }
        public AudioSessionState state {get; private set; }
        public Guid groupingParam {get; private set; }

        public AudioSession(string name_fp, AudioSessionState state_fp, Guid groupingParam_fp) : this()
        {
            this.name = name_fp;
            this.state = state_fp;
            this.groupingParam = groupingParam_fp;
        }
    }

    class Audio
    {
        public List<AudioSession> GetAudioSessionList()
        {
            List<AudioSession> sessionList = new List<AudioSession>();
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnumerator)
                    {
                        using (var audioMeterInformation = session.QueryInterface<AudioMeterInformation>())
                        using (var session2 = session.QueryInterface<AudioSessionControl2>())
                        {
                            if (session2 != null && session2.Process != null)
                            {
                                sessionList.Add(new AudioSession(session2.Process.ProcessName, session.SessionState, session.GroupingParam));
                            }
                        }
                    }
                }
            }
            return sessionList;
        }

        public bool IsAwkward(AudioSession defaultSession, float tolerance)
        {
            if (tolerance < 1) tolerance = 1;
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnumerator)
                    {
                        using (var audioMeterInformation = session.QueryInterface<AudioMeterInformation>())
                        {
                            if (audioMeterInformation.PeakValue > tolerance/1000
                                && !session.GroupingParam.Equals(defaultSession.groupingParam))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void UnmuteSession(AudioSession session)
        {
            Mute(session, false);
        }

        public void MuteSession(AudioSession session)
        {
            Mute(session, true);
        }


        private void Mute(AudioSession defaultSession, bool mute)
        {
            using (var sessionManager = GetDefaultAudioSessionManager2(DataFlow.Render))
            {
                using (var sessionEnumerator = sessionManager.GetSessionEnumerator())
                {
                    foreach (var session in sessionEnumerator)
                    {
                        if (session.GroupingParam == defaultSession.groupingParam)
                        {
                            using (var simpleVolume = session.QueryInterface<SimpleAudioVolume>())
                            {
                                simpleVolume.IsMuted = mute;
                            }
                        }
                    }
                }
            }
        }

        private static AudioSessionManager2 GetDefaultAudioSessionManager2(DataFlow dataFlow)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                using (var device = enumerator.GetDefaultAudioEndpoint(dataFlow, Role.Multimedia))
                {
                    return AudioSessionManager2.FromMMDevice(device);
                }
            }
        }
    }
}
