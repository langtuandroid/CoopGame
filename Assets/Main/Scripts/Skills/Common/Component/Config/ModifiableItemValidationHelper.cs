using Main.Scripts.Modifiers;

namespace Main.Scripts.Skills.Common.Component.Config
{
    public static class ModifiableItemValidationHelper
    {
        public static T[] GetLimitedArray<T>(ModifierBase? modifier, T[] dataArray)
        {
            if (modifier != null && modifier is ModifierId modifierId)
            {
                var variantsCount = modifierId.UpgradeLevels + 1;

                if (variantsCount != dataArray.Length)
                {
                    var limitedArray = new T[variantsCount];
                    for (var i = 0; i < dataArray.Length && i < limitedArray.Length; i++)
                    {
                        limitedArray[i] = dataArray[i];
                    }

                    return limitedArray;
                }
            }

            return dataArray;
        }
    }
}