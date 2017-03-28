/*
 * Copyright (c) 2006-2016, openmetaverse.co
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.co nor the names
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System.Text;

namespace OpenMetaverse.Voice
{
    public partial class VoiceGateway
    {
        /// <summary>
        /// This is used to login a specific user account(s). It may only be called after
        /// Connector initialization has completed successfully
        /// </summary>
        /// <param name="connectorHandle">Handle returned from successful Connector �create� request</param>
        /// <param name="accountName">User's account name</param>
        /// <param name="accountPassword">User's account password</param>
        /// <param name="audioSessionAnswerMode">Values may be �AutoAnswer� or �VerifyAnswer�</param>
        /// <param name="accountURI">""</param>
        /// <param name="participantPropertyFrequency">This is an integer that specifies how often
        /// the daemon will send participant property events while in a channel. If this is not set
        /// the default will be �on state change�, which means that the events will be sent when
        /// the participant starts talking, stops talking, is muted, is unmuted.
        /// The valid values are:
        /// 0 � Never
        /// 5 � 10 times per second
        /// 10 � 5 times per second
        /// 50 � 1 time per second
        /// 100 � on participant state change (this is the default)</param>
        /// <param name="enableBuddiesAndPresence">false</param>
        /// <returns></returns>
        public int AccountLogin (string connectorHandle, string accountName, string accountPassword, string audioSessionAnswerMode,
                                 string accountURI, int participantPropertyFrequency, bool enableBuddiesAndPresence)
        {
            StringBuilder sb = new StringBuilder ();
            sb.Append (MakeXML ("ConnectorHandle", connectorHandle));
            sb.Append (MakeXML ("AccountName", accountName));
            sb.Append (MakeXML ("AccountPassword", accountPassword));
            sb.Append (MakeXML ("AudioSessionAnswerMode", audioSessionAnswerMode));
            sb.Append (MakeXML ("AccountURI", accountURI));
            sb.Append (MakeXML ("ParticipantPropertyFrequency", participantPropertyFrequency.ToString ()));
            sb.Append (MakeXML ("EnableBuddiesAndPresence", enableBuddiesAndPresence ? "true" : "false"));
            sb.Append (MakeXML ("BuddyManagementMode", "Application"));
            return Request ("Account.Login.1", sb.ToString ());
        }

        /// <summary>
        /// This is used to logout a user session. It should only be called with a valid AccountHandle.
        /// </summary>
        /// <param name="accountHandle">Handle returned from successful Connector �login� request</param>
        /// <returns></returns>
        public int AccountLogout (string accountHandle)
        {
            string RequestXML = MakeXML ("AccountHandle", accountHandle);
            return Request ("Account.Logout.1", RequestXML);
        }
    }
}