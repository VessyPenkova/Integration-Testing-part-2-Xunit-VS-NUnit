using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using MoviesLibraryAPI.Controllers;
using MoviesLibraryAPI.Controllers.Contracts;
using MoviesLibraryAPI.Data.Models;
using MoviesLibraryAPI.Services;
using MoviesLibraryAPI.Services.Contracts;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace MoviesLibraryAPI.XUnitTests
{
    public class XUnitIntegrationTests : IClassFixture<DatabaseFixture>
    {
        private readonly MoviesLibraryXUnitTestDbContext _dbContext;
        private readonly IMoviesLibraryController _controller;
        private readonly IMoviesRepository _repository;

        public XUnitIntegrationTests(DatabaseFixture fixture)
        {
            _dbContext = fixture.DbContext;
            _repository = new MoviesRepository(_dbContext.Movies);
            _controller = new MoviesLibraryController(_repository);
        }

        [Fact]
        public async Task AddMovieAsync_WhenValidMovieProvided_ShouldAddToDatabase()
        {
            // Arrange
            var movieWheValid = new Movie
            {
                Title = "Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 120,
                Rating = 7.5
            };

            // Act
            await _controller.AddAsync(movieWheValid);

            // Assert
            var resultMovie = await _dbContext.Movies.Find(m => m.Title == "Test Movie").FirstOrDefaultAsync();
            Xunit.Assert.NotNull(resultMovie);
            Xunit.Assert.Equal("Test Movie", resultMovie.Title);
            Xunit.Assert.Equal("Test Director", resultMovie.Director);
            Xunit.Assert.Equal(2022, resultMovie.YearReleased);
            Xunit.Assert.Equal("Action", resultMovie.Genre);
            Xunit.Assert.Equal(120, resultMovie.Duration);
            Xunit.Assert.Equal(7.5, resultMovie.Rating);
        }

        [Fact]
        public async Task AddMovieAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var addWheninvalidMovieWhenInvalid = new Movie
            {

                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Act and Assert
            // Expect a ValidationException because the movie is missing a required field

            // Act and Assert
            // This is also work
            //_ = Xunit.Assert.ThrowsAsync<ValidationException>(() => _controller.AddAsync(invalidMovie));
            //Validating Exception Message
            var exception = await Xunit.Assert.ThrowsAsync<ValidationException>(() => _controller.AddAsync(addWheninvalidMovieWhenInvalid));
            Xunit.Assert.Equal("Movie is not valid.", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenValidTitleProvided_ShouldDeleteMovie()
        {
            // Arrange
            var movie = new Movie
            {
                Title = "Test Second Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(movie);


            // Act
            await _controller.DeleteAsync(movie.Title);

            // Assert
            var resultMovie = await _dbContext.Movies.Find(m => m.Title == "Test Movie").FirstOrDefaultAsync();
            Xunit.Assert.Null(resultMovie);
        }

        [Xunit.Theory]
        [Xunit.InlineData(null)]
        [Xunit.InlineData("")]
        [Xunit.InlineData("   ")]
        public async Task DeleteAsync_WhenTitleIsEmpty_ShouldThrowArgumentException(string invalidName)
        {
            Xunit.Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(invalidName));          
        }

        [Fact]
        public async Task DeleteAsync_WhenTitleDoesNotExist_ShouldThrowInvalidOperationException()
        {
            // Act and Assert
            Xunit.Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync("invalidName"));
        }

        [Fact]
        public async Task GetAllAsync_WhenNoMoviesExist_ShouldReturnEmptyList()
        {
            // Act
            var result = await _controller.GetAllAsync();

            // Assert
            Xunit.Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WhenMoviesExist_ShouldReturnAllMovies()
        {
            // Arrange
            var firstMovie = new Movie
            {
                Title = "First Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var secondMovie = new Movie
            {
                Title = "Second Test Movie",
                Director = "Test Director1",
                YearReleased = 2023,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(firstMovie);
            await _controller.AddAsync(secondMovie);

            // Act
            var allmovies = await _controller.GetAllAsync();
            // Assert
            // Ensure that all movies are returned
            Xunit.Assert.NotNull(allmovies);
            Xunit.Assert.Equal(2, allmovies.Count());
            var hasFirstMovie = allmovies.Any(x => x.Title == firstMovie.Title);
            var hasSecondMovie = allmovies.Any(x => x.Title == secondMovie.Title);
        }

        [Fact]
        public async Task GetByTitle_WhenTitleExists_ShouldReturnMatchingMovie()
        {
            // Arrange
            var firstMovie1 = new Movie
            {
                Title = "First Test Movie When Title Exist",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(firstMovie1);
            // Act
            var result = await _controller.GetByTitle(firstMovie1.Title);

            // Assert
            Xunit.Assert.NotNull(result);
            Xunit.Assert.Equal(firstMovie1.Title, result.Title);
            Xunit.Assert.Equal(firstMovie1.Director, result.Director);
            Xunit.Assert.Equal(firstMovie1.YearReleased, result.YearReleased);
            Xunit.Assert.Equal(firstMovie1.Genre, result.Genre);
            Xunit.Assert.Equal(firstMovie1.Duration, result.Duration);
            Xunit.Assert.Equal(firstMovie1.Rating, result.Rating);
        }

        [Fact]
        public async Task GetByTitle_WhenTitleDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _controller.GetByTitle("This Title does not exist");
            // Assert
            Xunit.Assert.Null(result);
        }


        [Fact]
        public async Task SearchByTitleFragmentAsync_WhenTitleFragmentExists_ShouldReturnMatchingMovies()
        {
            // Arrange
            var firstMovieSearchFragment = new Movie
            {
                Title = "First Search Fragment Test Movie",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var secondMovieSearchFragment = new Movie
            {
                Title = "Second Search Fragment Test Movie",
                Director = "Test Director1",
                YearReleased = 2023,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _dbContext.Movies.InsertManyAsync(new[] { firstMovieSearchFragment, secondMovieSearchFragment });
            //Act
            var result = await _controller.SearchByTitleFragmentAsync("Second");

            //Asser
            //Xunit.Assert.Equal(1, result.Count());
            var movieResult = result.First();
            Xunit.Assert.Equal(secondMovieSearchFragment.Title, movieResult.Title);
            Xunit.Assert.Equal(secondMovieSearchFragment.Director, movieResult.Director);
            Xunit.Assert.Equal(secondMovieSearchFragment.YearReleased, movieResult.YearReleased);
            Xunit.Assert.Equal(secondMovieSearchFragment.Genre, movieResult.Genre);
            Xunit.Assert.Equal(secondMovieSearchFragment.Duration, movieResult.Duration);
            Xunit.Assert.Equal(secondMovieSearchFragment.Rating, movieResult.Rating);
        }

        [Fact]
        public async Task SearchByTitleFragmentAsync_WhenNoMatchingTitleFragment_ShouldThrowKeyNotFoundException()
        {
            // Act and Assert
            Xunit.Assert.ThrowsAnyAsync<KeyNotFoundException>(() => _controller.SearchByTitleFragmentAsync("Non Existing Fragment"));
        }

        [Fact]
        public async Task UpdateAsync_WhenValidMovieProvided_ShouldUpdateMovie()
        {
            // Arrange
            var firstMovieToUpdate = new Movie
            {
                Title = "First Movie to Update",
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            var secondMovieToUpdate = new Movie
            {
                Title = "Second Movie to update",
                Director = "Test Director1",
                YearReleased = 2023,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };
            await _controller.AddAsync(firstMovieToUpdate);
            await _controller.AddAsync(secondMovieToUpdate);

            // Modify the movie
            firstMovieToUpdate.Title = $"{firstMovieToUpdate.Title} - Updated";

            // Act
            await _controller.UpdateAsync(firstMovieToUpdate);

            // Assert
            var result = await _dbContext.Movies.Find(x => x.Title == firstMovieToUpdate.Title).FirstOrDefaultAsync();
           Xunit.Assert.NotNull(result);
           
        }

        [Fact]
        public async Task UpdateAsync_WhenInvalidMovieProvided_ShouldThrowValidationException()
        {
            // Arrange
            var invalidMovieXunit = new Movie
            {
                Director = "Test Director",
                YearReleased = 2022,
                Genre = "Action",
                Duration = 86,
                Rating = 7.5
            };

            // Movie without required fields

            // Act and Assert
            Xunit.Assert.ThrowsAsync<ValidationException>(() => _controller.UpdateAsync(invalidMovieXunit));
        }
    }
}
