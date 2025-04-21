using RegridMapper.Models;

namespace RegridMapper.Core.Services
{
    public class StateDataService
    {
        private static IEnumerable<US_County>? _counties;
        private static IEnumerable<US_State>? _states;

        public StateDataService()
        {

        }

        public async Task<IEnumerable<US_State>> GetAllStates()
        {
            await Task.CompletedTask;

            _states ??= StateCountyDataFactory.AllStates();

            return _states;
        }
    }
}
