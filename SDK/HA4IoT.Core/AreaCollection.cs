using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Actuators;

namespace HA4IoT.Core
{
    public class AreaCollection : GenericControllerCollection<AreaId, IArea>
    { 
        ////private readonly Dictionary<AreaId, IArea> _areas = new Dictionary<AreaId, IArea>();

        ////public void Add(IArea area)
        ////{
        ////    if (area == null) throw new ArgumentNullException(nameof(area));

        ////    if (_areas.ContainsKey(area.Id))
        ////    {
        ////        throw new InvalidOperationException("Area with ID '" + area.Id + "' aready registered.");
        ////    }

        ////    _areas[area.Id] = area;
        ////}

        ////public IArea this[AreaId id]
        ////{
        ////    get
        ////    {
        ////        if (id == null) throw new ArgumentNullException(nameof(id));

        ////        IArea room;
        ////        if (!_areas.TryGetValue(id, out room))
        ////        {
        ////            throw new InvalidOperationException("Area with ID '" + id + "' is not registered.");
        ////        }

        ////        return room;
        ////    }
        ////}

        ////public IList<IArea> GetAll()
        ////{
        ////    return _areas.Values.ToList();
        ////}
    }
}
