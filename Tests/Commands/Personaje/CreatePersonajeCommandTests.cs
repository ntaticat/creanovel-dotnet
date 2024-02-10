using Application.Commands.Personaje;
using Xunit;
using Moq;
using Persistence;

namespace Tests.Commands.Personaje;

public class CreatePersonajeCommandTests
{

    [Fact]
    public void Handle_Should_CreatePersonajeSuccessfully()
    {
        // Arrange
        var command = new CreatePersonajeCommand.CreatePersonajeCommandRequest { Nombre = "Rafael" };
        // var handler = new CreatePersonajeCommand.Handler();
        // Act

        // Assert
    }
}