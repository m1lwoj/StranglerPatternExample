using LegacyCode;
using Polly;
using Refit;
using System.Net;

namespace StranglerPatternExample
{
    internal class ReservationProxyService : IReservationProxyService
    {
        private readonly IAccommodationService _accommodationService;
        private readonly IReservationService _reservationService;
        private readonly IFeatureFlagService _featureFlagService;

        public ReservationProxyService(IAccommodationService accommodationService, 
            IReservationService reservationService,
            IFeatureFlagService featureFlagService)
        {
            _accommodationService = accommodationService;
            _reservationService = reservationService;
            _featureFlagService = featureFlagService;
        }

        public void BookApartments()
        {
            BookInNewServiceWithLegacyServiceFallback();

            //BookInBothServices();
            //BookWithFeatureFlag();
        }

        /// <summary>
        /// Use a feature flag deciding whether to use new api or the old service.
        /// </summary>
        private void BookWithFeatureFlag()
        {
            if (_featureFlagService.IsAccommodationApiEnabled)
            {
                _accommodationService.BookApartment();
            }
            else
            {
                _reservationService.BookApartment();
            }
        }

        /// <summary>
        /// Run both services and compare the output after some time to confirm that the new one works fine. 
        /// </summary>
        private void BookInBothServices()
        {
            _reservationService.BookApartment();
            _accommodationService.BookApartment();
        }

        public void BookApartment()
        {
        }

        /// <summary>
        /// Run new service with a fallback of old service- so we have always safety buffer when something fall in the new one.
        /// </summary>
        private void BookInNewServiceWithLegacyServiceFallback()
        {
            //Third approach
            //Try to connect 3 times to api
            var retryPolicy = Policy
                .Handle<ApiException>(ex => ex.StatusCode == HttpStatusCode.RequestTimeout)
                .Retry(3, (exception, retryCount) => Task.Delay(500).ConfigureAwait(false));

            //If it fails, use old reservation service for booking the apartment
            var fallbackPolicy = Policy
                .Handle<Exception>()
                .Fallback(() => _reservationService.BookApartment());

            fallbackPolicy
                .Wrap(retryPolicy)
                .Execute(() => _accommodationService.BookApartment());
        }

        public void BookFlight()
        {
            throw new NotImplementedException();
        }

        public void RentCar()
        {
            throw new NotImplementedException();
        }
    }
}
