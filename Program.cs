using Autofac;
using BenMakesGames.PlayPlayMini;
using BenMakesGames.PlayPlayMini.Model;
using battleship.GameStates;

// TODO: any pre-req setup, ex:
/*
 * var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
 * var appDataGameDirectory = @$"{appData}{Path.DirectorySeparatorChar}battleship";
 * 
 * Directory.CreateDirectory(appDataGameDirectory);
 */

var gsmBuilder = new GameStateManagerBuilder();

gsmBuilder
    .SetWindowSize(1366, 768, 1)
    .SetInitialGameState<Startup>()

    // TODO: set a better window title
    .SetWindowTitle("battleship")

    // TODO: add any resources needed (refer to PlayPlayMini documentation for more info)
    .AddAssets(new IAsset[]
    {
        new FontMeta("Font","Font",11,23),
        // new PictureMeta(...)
        new SpriteSheetMeta("Cell","Cell",64,64),
        new SpriteSheetMeta("Miss","Miss",64,64),
        new SpriteSheetMeta("Hit","Hit",64,64),
        new SpriteSheetMeta("Mark","Mark",64,64),
        // new SongMeta(...)
        // new SoundEffectMeta(...)
    })

    // TODO: any additional service registration (refer to PlayPlayMini and/or Autofac documentation for more info)
    .AddServices(s =>
    {

    })
;

gsmBuilder.Run();