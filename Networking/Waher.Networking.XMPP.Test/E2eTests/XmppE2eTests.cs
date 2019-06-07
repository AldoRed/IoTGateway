﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.Sniffers;
using Waher.Networking.XMPP.P2P;
using Waher.Networking.XMPP.P2P.E2E;
using Waher.Networking.XMPP.P2P.SymmetricCiphers;

namespace Waher.Networking.XMPP.Test.E2eTests
{
    public abstract class XmppE2eTests : CommunicationTests
    {
        private IE2eEndpoint[] endpoints1;
        private IE2eEndpoint[] endpoints2;
        private EndpointSecurity endpointSecurity1;
        private EndpointSecurity endpointSecurity2;

        public override void PrepareClient1(XmppClient Client)
        {
            base.PrepareClient1(Client);
            this.endpointSecurity1 = new EndpointSecurity(this.client1, 128, this.endpoints1);
        }

        public override void PrepareClient2(XmppClient Client)
        {
            base.PrepareClient2(Client);
            this.endpointSecurity2 = new EndpointSecurity(this.client2, 128, this.endpoints2);
        }

        public override void ConnectClients()
        {
            base.ConnectClients();

            this.SubscribedTo(this.client1, this.client2);
            this.SubscribedTo(this.client2, this.client1);
        }

        private void SubscribedTo(XmppClient From, XmppClient To)
        {
            RosterItem Item1 = From.GetRosterItem(To.BareJID);
            RosterItem Item2 = To.GetRosterItem(From.BareJID);
            if (Item1 is null || (Item1.State != SubscriptionState.Both && Item1.State != SubscriptionState.To) ||
                Item2 is null || (Item2.State != SubscriptionState.Both && Item2.State != SubscriptionState.From))
            {
                ManualResetEvent Done2 = new ManualResetEvent(false);
                ManualResetEvent Error2 = new ManualResetEvent(false);

                To.OnPresenceSubscribe += (sender, e) =>
                {
                    if (e.FromBareJID == From.BareJID)
                    {
                        e.Accept();
                        Done2.Set();
                    }
                    else
                    {
                        e.Decline();
                        Error2.Set();
                    }
                };

                From.RequestPresenceSubscription(To.BareJID);

                Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done2, Error2 }, 10000));
            }
        }

        public abstract IE2eEndpoint GenerateEndpoint(IE2eSymmetricCipher Cipher);

        [TestMethod]
        public void Test_01_Message_AES()
        {
            this.Test_Message(
                new IE2eEndpoint[] { this.GenerateEndpoint(new Aes256()) },
                new IE2eEndpoint[] { this.GenerateEndpoint(new Aes256()) });
        }

        [TestMethod]
        public void Test_02_Message_ChaCha20()
        {
            this.Test_Message(
                new IE2eEndpoint[] { this.GenerateEndpoint(new ChaCha20()) },
                new IE2eEndpoint[] { this.GenerateEndpoint(new ChaCha20()) });
        }

        [TestMethod]
        public void Test_03_Message_AEAD_ChaCha20_Poly1305()
        {
            this.Test_Message(
                new IE2eEndpoint[] { this.GenerateEndpoint(new AeadChaCha20Poly1305()) },
                new IE2eEndpoint[] { this.GenerateEndpoint(new AeadChaCha20Poly1305()) });
        }

        private void Test_Message(IE2eEndpoint[] Endpoints1, IE2eEndpoint[] Endpoints2)
        {
            this.endpoints1 = Endpoints1;
            this.endpoints2 = Endpoints2;

            this.ConnectClients();
            try
            {
                ManualResetEvent Done = new ManualResetEvent(false);
                ManualResetEvent Error = new ManualResetEvent(false);

                this.client2.OnNormalMessage += (sender, e) =>
                {
                    if (e.Body == "Test message" && e.Subject == "Subject" && e.Id == "1")
                        Done.Set();
                    else
                        Error.Set();
                };

                this.endpointSecurity1.SendMessage(this.client1, E2ETransmission.AssertE2E,
                    QoSLevel.Unacknowledged, MessageType.Normal, "1", this.client2.FullJID,
                    "<test/>", "Test message", "Subject", "en", string.Empty, string.Empty,
                    null, null);

                Assert.AreEqual(0, WaitHandle.WaitAny(new WaitHandle[] { Done, Error }, 2000));
            }
            finally
            {
                this.endpointSecurity1?.Dispose();
                this.endpointSecurity2?.Dispose();

                this.DisposeClients();
            }
        }

    }
}
