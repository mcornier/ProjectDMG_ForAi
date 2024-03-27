using Newtonsoft.Json;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Images;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static OpenAI_API.Chat.ChatMessage;
using static System.Net.Mime.MediaTypeNames;

namespace ProjectDMG.OpenAIApi;

public class ClientLlmPlayer
{
    const string SystemPrompt =
        "Given a screenshot from a Pokémon game on the Game Boy, analyze the image and respond with the most suitable action to take (Up, Down, Left, Right, A, B, Start, Select), provide a brief description of the current scene in the game, and identify the game's current state (Menu, World, Battle). Please format your response as a JSON object with the keys \"selectedInput\", \"imageDescription\", and \"currentState\".";

    private OpenAIAPI _api;

    public ClientLlmPlayer()
    {
        // Suppose que l'API locale n'utilise pas de clé API pour l'authentification.
        _api = new OpenAIAPI(new APIAuthentication("no key for local api"));
        _api.ApiUrlFormat = "http://localhost:1234/v1";
    }

    //public async Task<GameboyInputs> CallLlmAsync(byte[] imageData)
    //{
    //    // Créez un message système avec votre instruction et l'entrée d'image.
    //    // Note : Cette approche suppose l'existence d'une surcharge adaptée à vos besoins dans l'API locale.
    //    //var systemMessage = new ChatMessage(SystemPrompt, new ImageInput(imageData));

    //    // Simulez l'envoi de ce message à votre API locale et obtenez la réponse.
    //    // Remplacer 'await _api.CallYourLocalVisionAPIAsync(systemMessage)' par l'appel réel à votre API locale.
    //    var response = await _api.Chat.CreateChatCompletionAsync(systemMessage);

    //    // Analyser la réponse
    //    //var response = await chat.GetResponseFromChatbotAsync();
    //    var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);
    //    string selectedInput = jsonResponse?.selectedInput;

    //    // Convertir la chaîne d'entrée sélectionnée en enum GameboyInputs
    //    return Enum.TryParse<GameboyInputs>(selectedInput, true, out var input) ? input : GameboyInputs.None;
    //}
}

//public class ClientLlmPlayer
//{
//    const string SystemPrompt =
//        "Given a screenshot from a Pokémon game on the Game Boy, analyze the image and respond with the most suitable action to take (Up, Down, Left, Right, A, B, Start, Select), provide a brief description of the current scene in the game, and identify the game's current state (Menu, World, Battle). Please format your response as a JSON object with the keys \"selectedInput\", \"imageDescription\", and \"currentState\"."
//        + "\nJson example :"
//        + "\n{"
//        + "\n  \"selectedInput\": \"A\","
//        + "\n  \"imageDescription\": \"The screen shows a Pokémon battle scene. A wild Pikachu faces off against the player's Squirtle. The player's Pokémon menu is open, with options to Fight, Bag, Pokémon, and Run.\","
//        + "\n  \"currentState\": \"Battle\""
//        + "\n}";

//    private OpenAI_API.OpenAIAPI _api;

//    public ClientLlmPlayer()
//    {
//        _api = new OpenAI_API.OpenAIAPI("no key for local api");
//        _api.ApiUrlFormat = "http://localhost:1234/v1";
//    }

//    public async Task<GameboyInputs> CallLlm(byte[] imageData)
//    {
//        //var chatRequest = new ChatRequest
//        //{
//        //    Model = "default",
//        //    Temperature = 0.7,
//        //    TopP = 1,
//        //    NumChoicesPerMessage = 1,
//        //    MaxTokens = 150,
//        //    FrequencyPenalty = 0,
//        //    PresencePenalty = 0,
//        //    Messages = new List<ChatMessage> { new ChatMessage { Role = ChatMessageRole.System, Content = SystemPrompt } }
//        //};

//        var ii = new ChatMessage.ImageInput(imageData);
//        var chatMessage = new ChatMessage(ChatMessageRole.System, SystemPrompt, ii);

//        var response = await _api.Chat.CreateChatCompletionAsync(chatMessage);

//        // Assuming the response is properly formatted as per the SystemPrompt
//        var jsonResponse = JsonSerializer.Deserialize<dynamic>(response..Last().Content);
//        string selectedInput = jsonResponse?.selectedInput;

//        return selectedInput switch
//        {
//            "Right" => GameboyInputs.Right,
//            "Left" => GameboyInputs.Left,
//            "Up" => GameboyInputs.Up,
//            "Down" => GameboyInputs.Down,
//            "A" => GameboyInputs.A,
//            "B" => GameboyInputs.B,
//            "Select" => GameboyInputs.Select,
//            "Start" => GameboyInputs.Start,
//            _ => GameboyInputs.None,
//        };
//    }
//}

public class ClientGptPlayer
{
    const string SystemPrompt =
        "Given a screenshot from a Pokémon game on the Game Boy, analyze the image and respond with the most suitable action to take (Up, Down, Left, Right, A, B, Start, Select), provide a brief description of the current scene in the game, and identify the game's current state (Menu, World, Battle). Please format your response as a JSON object with the keys \"selectedInput\", \"imageDescription\", and \"currentState\".";

    private OpenAIAPI _api;
    private bool isCallingApi;

    public bool IsCallingApi { get => isCallingApi;  }

    public ClientGptPlayer(string apiKey)
    {
        isCallingApi = false;
        _api = new OpenAIAPI(new APIAuthentication(apiKey));
    }

    public async Task<GameboyInputs> CallLlmAsync(byte[] imageData)
    {
        isCallingApi = true;

        var chat = _api.Chat.CreateConversation();
        chat.Model = Model.GPT4_Vision; // Utilisez le bon modèle GPT Vision ici
        chat.AppendSystemMessage(SystemPrompt);

        // Ajouter l'entrée d'image à la conversation
        chat.AppendUserInput("", new ImageInput(imageData));

        // Obtenir la réponse du modèle
        var response = await chat.GetResponseFromChatbotAsync();

        isCallingApi = false;
        var jsonResponse = JsonConvert.DeserializeObject<dynamic>(response);

        // Analyser la réponse JSON pour extraire l'entrée de jeu
        string selectedInput = jsonResponse?.selectedInput;

        // Convertir la chaîne d'entrée en énumération GameboyInputs
        return Enum.TryParse<GameboyInputs>(selectedInput, out var input) ? input : GameboyInputs.None;
    }
}