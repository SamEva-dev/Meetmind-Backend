using FluentAssertions;
using Meetmind.Domain.Entities;
using Meetmind.Domain.Enums;

namespace MeetingTests
{
    public class MeetingDomainTests
    {
        [Fact]
        public void Start_Should_Set_State_To_Recording()
        {
            var meeting = new Meeting("Sprint Review", DateTime.UtcNow);
            meeting.Start();

            meeting.State.Should().Be(MeetingState.Recording);
        }

        [Fact]
        public void Pause_Should_Only_Work_When_Recording()
        {
            var meeting = new Meeting("Daily", DateTime.UtcNow);
            meeting.Start();
            meeting.Pause();

            meeting.State.Should().Be(MeetingState.Paused);
        }

        [Fact]
        public void Resume_Should_Only_Work_When_Paused()
        {
            var meeting = new Meeting("Retro", DateTime.UtcNow);
            meeting.Start();
            meeting.Pause();
            meeting.Resume();

            meeting.State.Should().Be(MeetingState.Recording);
        }

        [Fact]
        public void Stop_Should_Set_EndUtc_And_State()
        {
            var meeting = new Meeting("Check-in", DateTime.UtcNow);
            meeting.Start();
            var endTime = DateTime.UtcNow.AddMinutes(30);
            meeting.Stop(endTime);

            meeting.State.Should().Be(MeetingState.Done);
            meeting.EndUtc.Should().Be(endTime);
            meeting.Duration.Should().BeCloseTo(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Start_Twice_Should_Throw()
        {
            var meeting = new Meeting("Demo", DateTime.UtcNow);
            meeting.Start();
            var act = () => meeting.Start();

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
