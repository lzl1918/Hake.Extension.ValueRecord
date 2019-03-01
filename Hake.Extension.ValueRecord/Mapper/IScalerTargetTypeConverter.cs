#define PROPERTY_PUBLIC_ONLY


namespace Hake.Extension.ValueRecord.Mapper
{
    public interface IScalerTargetTypeConverter<TTarget, TScaler>
    {
        TScaler ConvertToScaler(TTarget value);

        TTarget ConvertToTarget(TScaler value);
    }
}
