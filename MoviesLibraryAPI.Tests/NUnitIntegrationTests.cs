using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MoviesLibraryAPI.Controllers;
using MoviesLibraryAPI.Controllers.Contracts;
using MoviesLibraryAPI.Data.Models;
using MoviesLibraryAPI.Services;
using MoviesLibraryAPI.Services.Contracts;
using System.ComponentModel.DataAnnotations;

namespace MoviesLibraryAPI.Tests
{
    [TestFixture]
    public class NUnitIntegrationTests
    {
        private MoviesLibraryNUnitTestDbContext _dbContext;
        private IMoviesLibraryController _controller;
        private IMoviesRepository _repository;
        IConfiguration _configuration;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        [SetUp]
        public async Task Setup()
        {
            string dbName = $"MoviesLibraryTestDb_{Guid.NewGuid()}";
            _dbContext = new MoviesLibraryNUnitTestDbContext(_configuration, dbName);

            _repository = new MoviesRepository(_dbContext.Movies);
            _controller = new MoviesLibraryController(_repository);
        }

        [TearDown]
        public async Task TearDown()
        {
            await _dbContext.ClearDatabaseAsync();
        }

        [Test]
        public async Task AddMovieAsync_WhenValidMovieProvided_ShouldAddToDatabase()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Act
            await _controller.AddAsync(movie);

            // Assert
            var resultMovie = await _dbContext.Movies.Find(m => m.Title == "Test Movie").FirstOrDefaultAsync();
            Assert.IsNotNull(resultMovie);
        }

        [Test]
        public async Task AddMovieAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var invalidMovie = new Movie
            {
               
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Act and Assert
            // Expect a ValidationException because the movie is missing a required field
            var exception = Assert.ThrowsAsync<ValidationException>(() => _controller.AddAsync(invalidMovie));
        }

        [Test]
        public async Task DeleteAsync_WhenValidTitleProvided_ShouldDeleteMovie()
        {
            // Arrange            
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Act
            await _controller.AddAsync(movie);
            await _controller.DeleteAsync(movie.Title);

            // Assert
            var resultMovie = await _dbContext.Movies.Find(m => m.Title == "Test Movie").FirstOrDefaultAsync();
            Assert.Null(resultMovie);
        }


        [Test]
        public async Task DeleteAsync_WhenTitleIsNull_ShouldThrowArgumentException()
        {
            // Act and Assert
            Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(null));
            var exception = Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(null));
            Assert.That(exception.Message, Is.EqualTo("Title cannot be empty"));
        }

        [Test]
        public async Task DeleteAsync_WhenTitleIsEmpty_ShouldThrowArgumentException()
        {
            // Act and Assert
             Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(""));
        }

        [Test]
        public async Task DeleteAsync_WhenTitleDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Act and Assert
            const string nonExistingTitle = "Non existing Title";
            var exception = Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync(nonExistingTitle));
            Assert.That(exception.Message, Is.EqualTo($"Movie with title '{nonExistingTitle}' not found."));
        }

        [Test]
        public async Task GetAllAsync_WhenNoMoviesExist_ShouldReturnEmptyList()
        {
            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetAllAsync_WhenMoviesExist_ShouldReturnAllMovies()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var movie1 = new Movie
            {
                Title = "Test Movie2",
                Director = "Test Director1",
                YearReleased = 2023,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movie);
            await _controller.AddAsync(movie1);

            // Act
            var allmovies = await _controller.GetAllAsync();

            // Assert
            Assert.IsNotEmpty(allmovies);
            Assert.That(2, Is.EqualTo(allmovies.Count()));
            var hasFirstMovie = allmovies.Any(x => x.Title == movie.Title);
            var hasSecondMovie = allmovies.Any(x => x.Title == movie1.Title);

        }

        [Test]
        public async Task GetByTitle_WhenTitleExists_ShouldReturnMatchingMovie()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movie);
            // Act

            var result = await _controller.GetByTitle(movie.Title);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Title, Is.EqualTo(movie.Title));
            Assert.That(result.Director, Is.EqualTo(movie.Director));
            Assert.That(result.YearReleased, Is.EqualTo(movie.YearReleased));
            Assert.That(result.Genre, Is.EqualTo(movie.Genre));
            Assert.That(result.Duration, Is.EqualTo(movie.Duration));
            Assert.That(result.Rating, Is.EqualTo(movie.Rating));
        }

        [Test]
        public async Task GetByTitle_WhenTitleDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _controller.GetByTitle("TitleDoesNotExist");
            // Assert
            Assert.IsNull(result);
        }


        [Test]
        public async Task SearchByTitleFragmentAsync_WhenTitleFragmentExists_ShouldReturnMatchingMovies()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "First Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var movie1 = new Movie
            {
                Title = "Second Movie",
                Director = "Test Director1",
                YearReleased = 2023,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movie);
            await _controller.AddAsync(movie1);

            // Act
            var result = await _controller.SearchByTitleFragmentAsync("Movie");

            // Assert // Should return one matching movie
            Assert.IsNotNull(result);
            Assert.That(2, Is.EqualTo(result.Count()));

            var hasFirstMovie = result.Any(x => x.Title == movie.Title);
            var hasSecondMovie = result.Any(x => x.Title == movie1.Title);
        }

        [Test]
        public async Task SearchByTitleFragmentAsync_WhenNoMatchingTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
            Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.SearchByTitleFragmentAsync("HoHo"));
            //TODO: Assert Message
        }

        [Test]
        public async Task UpdateAsync_WhenValidMovieProvided_ShouldUpdateMovie()
        {
            // Arrange
            var movieToUptade = new Movie
            {
                Title = "First Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var movie1 = new Movie
            {
                Title = "Second Movie",
                Director = "Test Director1",
                YearReleased = 2023,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movieToUptade);
            await _controller.AddAsync(movie1);

            // Modify the movie
            movieToUptade.Title = $"{movieToUptade.Title} - Updated";

            // Act
            await _controller.UpdateAsync(movieToUptade);
            // Assert
            var result = await _dbContext.Movies.Find(x =>x.Title == movieToUptade.Title).FirstOrDefaultAsync();
            Assert.IsNotNull(result);
        }

        [Test]
        public async Task UpdateAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var invalidMovie = new Movie
            {

                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Movie without required fields

            // Act and Assert
            Assert.ThrowsAsync<ValidationException>(() => _controller.UpdateAsync(invalidMovie));
        }


        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _dbContext.ClearDatabaseAsync();
        }
    }
}
