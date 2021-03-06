﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Commands.Attributes;
using Bot.Infrastructure.AWS;

namespace Bot.Commands.AWS
{
    [AwsElbCommand("instances")]
    public class AwsElbInstancesCommand : IrcCommandProcessor
    {
        public override void Process(IrcCommand command)
        {
            base.Process(command);

            if (HandleNoParameters("I need the load balancer name.", null))
                return;

            var loadBalancerName = command.Parameters[0];
            var elb = new ELB();
            var ec2 = new EC2();
            var instances = elb.Instances(loadBalancerName);
            var outOfLB = ElbState.GetStates(loadBalancerName).ToDictionary(s => s.State.InstanceId, s => s);
            var descriptions = ec2.InstanceDescriptions(instances.Select(i => i.InstanceId));

            foreach (var description in descriptions)
            {
                var timeOutString = outOfLB.ContainsKey(description.InstanceId) ? "Out For: " + outOfLB[description.InstanceId].TimeSincePulled() : "InService";
                SendNotice(
                    string.Format(
                        @"id: {0} / ip: {1} / state: {2} / type: {3} / ami: {4} / dns: {5} / Elb State: {6}",
                        description.InstanceId,
                        description.IpAddress,
                        description.InstanceState.Name,
                        description.InstanceType,
                        description.ImageId,
                        description.PublicDnsName,
                        timeOutString
                    )
                );
            }
        }
    }
}
