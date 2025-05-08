using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Principles.Messages;

public class MsgThatProgressOfHabitUpdated( ProgressOfHabit value ) : ValueChangedMessage<ProgressOfHabit>( value );