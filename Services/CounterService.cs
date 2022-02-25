namespace WebApplicationService.Services
{
    public class CounterService
    {
        private int _counter;
        public CounterService()
        {
            _counter = 0;
        }

        public int Counter()
        {
            return _counter;
        }

        public void IncrementCounter()
        {
            _counter++;
        }
    }
}
