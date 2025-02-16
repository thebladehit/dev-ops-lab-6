using DevOpsProject.HiveMind.Logic.Services.Interfaces;
using DevOpsProject.HiveMind.Logic.State;
using DevOpsProject.Shared.Models;
using Microsoft.Extensions.Logging;

namespace DevOpsProject.HiveMind.Logic.Services
{
    public class HiveMindMovingService : IHiveMindMovingService
    {
        private readonly ILogger<HiveMindMovingService> _logger;
        private Timer _movementTimer;
        public HiveMindMovingService(ILogger<HiveMindMovingService> logger)
        {
            _logger = logger;
        }

        public void MoveToLocation(Location destination)
        {
            lock (typeof(HiveInMemoryState))
            {
                if (HiveInMemoryState.OperationalArea == null || HiveInMemoryState.CurrentLocation == null)
                {
                    _logger.LogWarning("Cannot start moving: OperationalArea or CurrentLocation is not set.");
                    return;
                }

                // If already moving - stop movement
                if (HiveInMemoryState.IsMoving)
                {
                    StopMovement();
                }

                HiveInMemoryState.Destination = destination;
                HiveInMemoryState.IsMoving = true;

                _logger.LogInformation($"Received move command: Moving towards {destination}");

                // Start the movement timer if not already running
                if (_movementTimer == null)
                {
                    // TODO: Recalculating position each N seconds
                    _movementTimer = new Timer(UpdateMovement, null, TimeSpan.Zero, TimeSpan.FromSeconds(3));
                    _logger.LogInformation("Movement timer started.");
                }
            }
        }

        private void UpdateMovement(object state)
        {
            lock (typeof(HiveInMemoryState))
            {
                var currentLocation = HiveInMemoryState.CurrentLocation;
                var destination = HiveInMemoryState.Destination;

                if (currentLocation == null || destination == null)
                {
                    StopMovement();
                    return;
                }

                if (AreLocationsEqual(currentLocation.Value, destination.Value))
                {
                    StopMovement();
                    return;
                }

                Location newLocation = CalculateNextPosition(currentLocation.Value, destination.Value, 0.1f);
                HiveInMemoryState.CurrentLocation = newLocation;

                _logger.LogInformation($"Moved closer: {newLocation}");
            }
        }

        private void StopMovement()
        {
            _movementTimer?.Dispose();
            _movementTimer = null;
            HiveInMemoryState.IsMoving = false;
            HiveInMemoryState.Destination = null;
            _logger.LogInformation("Movement stopped: Reached destination.");
        }

        private static bool AreLocationsEqual(Location loc1, Location loc2)
        {
            const float tolerance = 0.000001f;
            return Math.Abs(loc1.Latitude - loc2.Latitude) < tolerance &&
                   Math.Abs(loc1.Longitude - loc2.Longitude) < tolerance;
        }

        private static Location CalculateNextPosition(Location current, Location destination, float stepSize)
        {
            float newLat = current.Latitude + (destination.Latitude - current.Latitude) * stepSize;
            float newLon = current.Longitude + (destination.Longitude - current.Longitude) * stepSize;
            return new Location
            {
                Latitude = newLat,
                Longitude = newLon
            };
        }
    }
}
