﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using IrcDotNet;

namespace Bot.Commands
{
    public class IrcCommandProcessor : IIrcCommandProcessor
    {
        protected IrcCommand command;

        public IrcCommandProcessor() { }

        public virtual void Process(IrcCommand command)
        {
            this.command = command;
        }

        protected bool HasParameters { get { return this.command.Parameters.Length != 0; } }

        protected void SendMessage(string message)
        {
            if (this.command == null) return;
            if (string.IsNullOrEmpty(message)) return;

            this.command.Client.LocalUser.SendMessage(
                this.command.Target,
                message
            );
        }

        protected void SendMessages(IEnumerable<string> messages)
        {
            if (this.command == null) return;

            foreach (var message in messages)
                SendMessage(message);
        }

        protected void SendNotice(string message)
        {
            if (this.command == null) return;

            this.command.Client.LocalUser.SendNotice(
                this.command.Source as IIrcMessageTarget,
                message
            );
        }

        protected void SendNotices(IEnumerable<string> messages)
        {
            if (this.command == null) return;

            foreach (var message in messages)
                SendNotice(message);
        }

        protected void SendPrivateMessage(string message)
        {
            if (this.command == null) return;

            this.command.Client.LocalUser.SendMessage(
                this.command.Source as IIrcMessageTarget,
                message
            );
        }

        protected void SendPrivateMessages(IEnumerable<string> messages)
        {
            if (this.command == null) return;

            foreach (var message in messages)
                SendPrivateMessage(message);
        }

        protected bool HandleNoParameters(string prompt, IIrcCommandProcessor helpCommand, bool publicMessage = true)
        {
            if (!HasParameters)
            {
                if (publicMessage)
                    SendMessage(prompt);
                else
                    SendNotice(prompt);

                Thread.Sleep(1000);
                ShowHelp(helpCommand);
            }

            return !HasParameters;
        }

        protected void ShowHelp(IIrcCommandProcessor helpCommand)
        {
            if (helpCommand != null)
                helpCommand.Process(this.command);
        }

        protected bool HasAdminUser()
        {
            if (!command.HasUser)
            {
                SendMessage("You need to be logged in to use this command.");
                return false;
            }

            if (!command.User.IsAdmin)
            {
                SendMessage(
                    string.Format(
                        "I'm sorry, {0}, you need to be an administrator to use this command.",
                        command.Source.Name
                        )
                    );

                return false;
            }

            return true;
        }
    }
}
