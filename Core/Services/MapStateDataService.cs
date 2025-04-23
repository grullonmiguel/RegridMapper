using RegridMapper.Models;

namespace RegridMapper.Core.Services
{
    public class MapStateDataService
    {
        private static IEnumerable<US_County>? _counties;
        private static IEnumerable<US_State>? _states;

        public MapStateDataService() { }

        public async Task<IEnumerable<US_County>> GetAllCounties() 
            => _counties ??= await LoadCountiesAsync();

        public Task<IEnumerable<US_State>> GetAllStates() 
            => Task.FromResult(_states ??= MapStateDataFactory.AllStates());

        private Task<IEnumerable<US_County>> LoadCountiesAsync()
        {
            _states ??= MapStateDataFactory.AllStates();
            _counties = _states.SelectMany(state => state.Counties).ToList();
            return Task.FromResult(_counties);
        }
    }
}