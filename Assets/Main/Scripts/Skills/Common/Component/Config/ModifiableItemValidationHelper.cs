using Main.Scripts.Modifiers;

namespace Main.Scripts.Skills.Common.Component.Config
{
    public static class ModifiableItemValidationHelper
    {
        public static T[] GetLimitedArray<T>(ModifierId? modifierId, T[] dataArray)
        {
            if (modifierId != null && modifierId.LevelsCount + 1 != dataArray.Length)
            {
                var limitedArray = new T[modifierId.LevelsCount + 1];
                for (var i = 0; i < dataArray.Length && i < limitedArray.Length; i++)
                {
                    limitedArray[i] = dataArray[i];
                }

                return limitedArray;
            }

            return dataArray;
        }
    }
}