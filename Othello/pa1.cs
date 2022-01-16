#nullable enable
using System;
using static System.Console;

namespace pa1
{
	record Player( string Colour, string Symbol, string Name );
   
    static partial class Program
    {			
        static void Main( )
        {
            Welcome( );
            
            Player[ ] players = new Player[ ] 
            {
                NewPlayer( colour: "black", symbol: "X", defaultName: "Black" ),
                NewPlayer( colour: "white", symbol: "O", defaultName: "White" ),
            };
            WriteLine( );
            
            int turn = GetFirstTurn( players, defaultFirst: 0 );
            WriteLine( );
                       
            int rows = GetBoardSize( direction: "rows",    defaultSize: 8 );
            int cols = GetBoardSize( direction: "columns", defaultSize: 8 );
            
            string[ , ] game = NewBoard( rows, cols );
            //this boolean tells if board is only being tested for valid moves 
            //and makes sure that no flips are done
            bool validTest = false;
            
            // Play the game.
            Welcome( );

            bool gameOver = false;
            while( ! gameOver )
            {
                DisplayBoard( game ); 
                DisplayScores( game, players );
                
                string move = GetMove( players[ turn ] );
                if( move == "quit" ) gameOver = true;
                else
                {
                    bool madeMove = TryMove( game, players[ turn ], move, validTest );
                    if( madeMove ) turn = ( turn + 1 ) % players.Length;
                    else 
                    {
                        WriteLine( "Your choice didn't work!" );
                        WriteLine( "Press <Enter> to try again." );
                        ReadLine( ); 
                    }
                    gameOver = !validMoves( game, players[ turn ], validTest );
                    validTest = false;
                }
            }
            
            // Show fhe final results.
            DisplayWinners( game, players );
            WriteLine( );
        }
		
		 static void Welcome( )
        {
			WriteLine( "Welcome to Othello!" );
			WriteLine( );
			WriteLine( );
        }
         static Player NewPlayer( string colour, string symbol, string defaultName )
        {
			Write( $"Type the {colour} disc ({symbol}) player name [or <Enter> for '{defaultName}' ]: " );
			string response = ReadLine( );
			string name;
			if( response.Length < 1 ) name = defaultName;
			else name = response;

            return new Player( colour, symbol, name );
        }
        static int GetFirstTurn( Player[ ] players, int defaultFirst )
        {
			Write( $"Choose who will play first. Choose '0' for {players[ 0 ].Name} and '1' for {players[ 1 ].Name}: " );
			string response = ReadLine( );
			
			if( response.Length != 1 ) return defaultFirst;
			else
			{
				int numResponse = int.Parse( response );
				//return is 0 if p1 wants to start and 1 if p2 wants to start
				return numResponse;
			}
        }
        static int GetBoardSize( string direction, int defaultSize )
        {
            if( direction == "rows" )
            {
				WriteLine( "Enter the number of rows (must be even): " );
				string response = ReadLine( );
				int numOfRows;
				
				while( !Int32.TryParse( response, out numOfRows ) )
				{
					WriteLine( "Input must be a number. Please try again:" );
					response = ReadLine( );
				}
				numOfRows = int.Parse( response );
				
				while( numOfRows % 2 != 0 || numOfRows < 3 || numOfRows > 27 )
				{				
					WriteLine( "Please enter an even number of rows between 4 and 26: " );
					numOfRows = int.Parse( ReadLine( ) );
				}
				return numOfRows;
			}
			if( direction == "columns" )
			{
				WriteLine( "Enter the number of columns (must be even): "  );
				string response = ReadLine( );
				int numOfCols;
								
				while( !Int32.TryParse( response, out numOfCols ) )
				{
					WriteLine( "Input must be a number. Please try again:" );
					response = ReadLine( );
				}
				numOfCols = int.Parse( response );
				
				while( numOfCols % 2 != 0 || numOfCols < 3 || numOfCols > 27 )
				{				
					WriteLine( "Please enter an even number of columns between 6 and 24 (inclusive): " );
					numOfCols = int.Parse( ReadLine( ) );
				}
				return numOfCols;				
			}
			else return 8;
        }
        
        static string GetMove( Player player )
        {
			WriteLine( "How would {0} like to move? ( 'quit' for quit; 'skip' for skip )", player.Name );
			string response = ReadLine( );
			if( response == "quit" ) return "quit";
            if( response == "skip" ) return "skip";
            else return response;
        }
        
        
        // Try to make a move. Return true if it worked.
        static bool TryMove( string[ , ] board, Player player, string move, bool validTest )
        {
			// if move is skip return true (no action needed)
			if( move == "skip" ) return true;
			if( move == "" ) return false;
			string row = ( move.Substring( 0, 1 ) );
			int rowIndex = IndexAtLetter( row );
			//Write( rowIndex );
			
			string col = ( move.Substring( 1, 1 ) );
			int colIndex = IndexAtLetter( col );
			//Write( colIndex );
			
			// if move length is  2, both are letters
			if( move.Length != 2 ) return false;
			
			//check if move is outside of board			
			if( rowIndex >= board.GetLength( 0 ) || rowIndex < 0 ) return false;
			if( colIndex >= board.GetLength( 1 ) || colIndex < 0 ) return false;
			
			//check if board is blank
			if( board[ rowIndex, colIndex ] != " " ) return false;

			
			bool right		= TryDirection( board, player, rowIndex, 1,  colIndex, 0,  validTest );
			bool upRight	= TryDirection( board, player, rowIndex, 1,  colIndex, 1,  validTest );
			bool up			= TryDirection( board, player, rowIndex, 0,  colIndex, 1,  validTest );
			bool upLeft		= TryDirection( board, player, rowIndex, -1, colIndex, 1,  validTest );
			bool left		= TryDirection( board, player, rowIndex, -1, colIndex, 0,  validTest );
			bool downLeft	= TryDirection( board, player, rowIndex, -1, colIndex, -1, validTest );
			bool down		= TryDirection( board, player, rowIndex, 0,  colIndex, -1, validTest );
			bool downRight	= TryDirection( board, player, rowIndex, 1,  colIndex, -1, validTest );
			
			if( right || upRight || up || upLeft || left || downLeft || down || downRight ) return true;
			else return false;
        }
         // Do the flips along a direction specified by the row and column delta for one step.
        
        static bool TryDirection( string[ , ] board, Player player, int moveRow, int deltaRow, int moveCol, int deltaCol, bool validTest )
        {
			//if there is a neighbour( moveRow + delta row still on the board )
			if( moveRow + deltaRow >= board.GetLength( 0 ) || moveCol + deltaCol >= board.GetLength( 1 ) ||
				moveRow + deltaRow < 0 || moveCol + deltaCol < 0 ) return false;
			string tilePlaced = board[ moveRow + deltaRow, moveCol + deltaCol ];
			if( tilePlaced == player.Symbol || tilePlaced == " " ) return false; 
            
			//can i move another step? return false if it cant
			for( int i = 1; !( moveRow + i * deltaRow >= board.GetLength( 0 ) || moveCol + i * deltaCol >= board.GetLength( 1 ) ||
				moveRow + i * deltaRow < 0 || moveCol + i * deltaCol < 0 ) ; i ++ )
			{
				//if next step is my colour return true
				if( board[ moveRow + i * deltaRow, moveCol + i * deltaCol ] == player.Symbol ) 
				{
					if( !validTest )
					{
						for( int j = 1; j <= i ; j++)
						{
							board[ moveRow + j * deltaRow, moveCol + j * deltaCol ] = player.Symbol;	
						}
						board[ moveRow, moveCol ] = player.Symbol;
					}
					return true;
				}
				else if( board[ moveRow + i * deltaRow, moveCol + i * deltaCol ] == " " ) return false;
			}
			return false;
        }
        
        // Count the discs to find the score for a player.
        static int GetScore( string[ , ] board, Player player )
        {
			int scoreCount = 0;
			for( int i = 0; i < board.GetLength( 0 ); i ++ )
			{
				for( int j = 0; j < board.GetLength( 1 ); j ++ )
				{
					if( board[ i, j ] == player.Symbol ) scoreCount ++;
				}
			}
            return scoreCount;
        }
        
        // Display a line of scores for all players.
        
        //Player[ ] players is the entire array
        static void DisplayScores( string[ , ] board, Player[ ] players )
        {
			WriteLine( "Score Count:" );
			int player1Score = GetScore( board, players[ 0 ] ); // default black
			int player2Score = GetScore( board, players[ 1 ] ); // default white
			
			WriteLine( $"{players[ 0 ].Name}: {player1Score}" );
			WriteLine( $"{players[ 1 ].Name}: {player2Score}" );
        }
        // Display winner(s) and categorize their win over the defeated player(s).
        
        static void DisplayWinners( string[ , ] board, Player[ ] players )
        {
			//start with GetScore and DisplayScores
			int player1Score = GetScore( board, players[ 0 ] ); // default black
			int player2Score = GetScore( board, players[ 1 ] ); // default white
			
			DisplayBoard( board ); 
			DisplayScores( board, players );
			
			if( player1Score > player2Score ) WriteLine( $"{players[0].Name} won!" );
			else if( player2Score > player1Score ) WriteLine( $"{players[1].Name} won!" );
			else if( player2Score == player1Score ) WriteLine ( "There was a tie!" );
			
        }
        
        static bool validMoves( string[ , ] board, Player players, bool validTest )
        {
			string testMove;
			validTest = true;
			for( int i = 0; i < board.GetLength( 0 ); i ++ )
			{
				for( int j = 0; j < board.GetLength( 1 ); j ++ )
				{
					testMove = LetterAtIndex( i ) + LetterAtIndex( j );
					if( TryMove( board, players, testMove, validTest ) )
					{
						return true;
					}
				}
			}
			return false;
		}
        
    }
}
