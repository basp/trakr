namespace Trakr.Core;

internal static class FlowControlResolver
{
    public static bool TryResolve(Row row, int currentOrderIndex, out FlowControlTarget target)
    {
        int? destinationOrderIndex = null;
        int? destinationRowIndex = null;

        for (var channelIndex = 0; channelIndex < row.ChannelCount; channelIndex++)
        {
            var effect = row.GetChannelEvent(channelIndex)?.Effect;
            if (effect is null)
            {
                continue;
            }

            switch (effect.Code)
            {
                case "B":
                    destinationOrderIndex = effect.Parameter;
                    break;
                case "D":
                    destinationRowIndex = effect.Parameter;
                    break;
            }
        }

        if (destinationOrderIndex is null && destinationRowIndex is null)
        {
            target = default;
            return false;
        }

        target = new FlowControlTarget(destinationOrderIndex ?? currentOrderIndex + 1, destinationRowIndex);
        return true;
    }
}

internal readonly record struct FlowControlTarget(int OrderIndex, int? RowIndex);