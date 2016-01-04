using UnityEngine;
using System.Collections;

public class ToDo : MonoBehaviour {

	// TOP PRIORITY
	// 1. Conversation Engine
	
	// 2. 
	
	// 3. 
	
	// CONVERSATION ENGINE
	// Create scene for conversation test
	// Conversation will consist of 2 heads either side of screen and speech bubbles in the middle
	// What to use for 
	
	// TERRAIN
	// ? Things to jump over
	// ? Things to climb up
	// Remove pools in favour of habitats?
	// Add habitations - caves, huts, temples etc.
	// Create more differentiation between areas in the forest
	// -- colour areas differently - use different materials for ground (and path?)
	// -- vary the types of trees and bushes in each area
	// -- add one off features: waystones, unusual trees, ruins
	// Need to sort out something for the edges of the forest
	
	// CASTLE
	// create the castle's colour map and generate the layout
	// -- needs the antechamber, throne room, synne's room, gatehouse and connecting passages
	
	// GENERAL GRAPHICS
	// Shadows for forest objects
	// Shadows for player
	// - Get size of sprite and scale sphere caster to appropriate size.
	// Ground fog
	// Falling and flying particles
	// Camera to move up, closer and point down when at a pool/habitat? 
	
	// Characters will need 4 views - front, back, left and right 
	// ANIMALS
	// Birds - sitting and flying between trees
	// Fish, frogs, bugs in and around pools
	// Foxes, badgers, boars, rabbits, deer - need some AI to run away from player
	
	// PEOPLE
	// People will stay near their dwellings mostly
	// Do they leave and come back at certain times of day?
	
	// INTERACTIONS
	// How to start interaction with objects?
	// Inventory
	// How to use items, combine items
	// Potions? Crafting?
	// Pick up animation
	
	// ATMOSPHERE
	// Weather?
	// Day/Night cycle
	// Seasons
	
	// CUTSCENES?
	// -- introducing the tasks and characters
	
	/*
	Intro:
	The King summons his 4 children to the throne room. He declares that he is going to give his kingdom to the one who can prove their love
	the most.
	The children are:
	ÆÐELFRIÐ - the oldest son. ÆÐELFRIÐ rules the West, and is happy there. He does not want the crown, but realises that his cruel and wasteful
		younger brother desperately wants it. He will compete for the crown solely to prevent him from having it.
	EOFORHILD - the oldest daughter. EOFORHILD rules the East. She is well loved by her people and has proved herself many times in battle
		against invaders from the continent. However, she is stubborn and has never seen eye to eye with the King, and knows she is unlikely to
		be chosen to rule. This rift has made her bitter and distrustful.
	LEOFDÆG - the youngest son. LEOFDÆG is lazy, spiteful and cruel. He rules the North. He feels betrayed by his father, who he has always made
		a point of ingratiating himself with, for sending him there. LEOFDÆG will use any trick he can to beat his siblings. 
	SUNNGIFU (referred to as Synne by her siblings)- the youngest daughter and our player character. SUNNGIFU has a good relationship with all her
		siblings, and is the only one of them whom LEOFDÆG treats kindly (or rather, what appears kindness, as he doesn't see her as a threat).

	4 seasons:
	The game takes place over 4 seasons of the year. At the start of each season, the King issues a cryptic task to his children. These tasks are
		open to interpretation so can have several possible solutions. Each sibling is trying to get the best answer to the task, but their
		approaches may be different. There are a number of possible conversations the PC can have in the castle. Each character will speak to each
		other, barring the PC, who will have the choice to talk to a character directly, or overhear a conversation between two others.

	In each season, there are 3 solutions to each task. This will naturally bring the player into conflict with one of the other siblings. 
	Depending on the solution, this may result in the death of one of the siblings. Each character has a thread throughout the game, which changes
	based on their relationship to each of the other characters (and whether they are still alive!) There are 3 main points of interaction - before
	the task, during the task and after the task. Favours/grudges will only change during the after task section. For each task, barring the first,
	how each character behaves towards the others will change depending on their status. Their intent may also change.
	

	Spring: task - the heart of a wolf.
	1. There are tales of a mythical wolf whose heart, when consumed, is said to give insight into the characters of men. Kill it and bring back
		the heart. You will need to track, then drive it into a trap. Aedelfrith will undertake this task. If the player chooses this task, it is a
		straight head-to-head with Aedelfrith. He will not prevent the player, but the player will not be able to stop him with force.
		
	2. Eoforhild is referred to as the Wolf of the East. L will try to convince the PC that E intends to kill the king using poison and K knows this,
		hence the task. He will say that E will be looking for a poison in the forest, and that they must kill her before she can collect it. L will
		try to convince the player that this is the correct task, by telling the player of the resentment E shows her father. E does not help this
		situation, as she is very blunt with both PC and K. L will try to convince PC that he does not want the crown to hide his motive.
		
	3. A flower called Wolfsheart grows in the forest, said to cure any pain. The herbalist can tell you the location, for a price. E will undertake
		this task.
		
	Conversations:
	PC - A: A will tell the PC of the wolf, and his plan to hunt it. He will tell the PC that he would consider it an honour to contest for it.
	PC - E: E will tell the PC of the pain K suffers from and how the herbalist can help ease his pain, but also complains about her station in the
		East in a manner that suggests "easing his pain" may be final.
	PC - L: L will tell the PC that he doesn't trust E, and how she always complains about Ks decision to send her East. He tells the PC that A is
		a bully. He asks the PC for help, and tells them to meet him in the forest.
	A - E: They discuss how they don't trust L, but argue over their plans. Both their plans are revealed here.
	A - L: A tells L how he knows he's up to no good, and that he's watching him. He warns him that he will pay if he hurts his siblings.
	E - L: Es conversation with L is brief. Do not cross me.
	
	Outcomes:
	1a. PC takes kills the wolf, and brings back its heart. L tells K of the plot against him by E, and K forces E to drink the potion she has
		brought him. L has switched the potion with poison, and E dies. K shows favour to L. Either:
		i) The PC takes the heart, L gains favour with A, but PC discovers L's plot.
		ii) The PC gives the heart to A. L gains a grudge from A, but favour from A. 
	1b. PC loses the wolf's heart to A. As above, but A uses the power, and gains insight into L's plot.
	2a. PC kills E and takes the potion (which they do not know is a cureall), A brings back the wolf's heart. The PC gains a grudge from A for
		killing E. L tells the PC to give him the potion so he can dispose of it. Either:
		i) The PC gives the potion to L, who presents it to K and denounces the PC. K takes the wolf's heart, A is given favour. L gets favour from
		A. PC gains a grudge from K.
		ii) The PC refuses to give L the potion, and presents it to the king and gains favour. The PC gains a grudge from L as well as A.
	2b. PC does not in the end kill E. E presents the potion to the king. Thinking PC switched the potions, L tells K it is a plot to poison him, so
		K forces E to drink the potion. The potion restores E, and K gives her favour. A gives the wolf's heart to the PC, who gains a grudge from L.
	3a. PC collects the wolfsheart, and makes the potion before E does. Leofdaeg switches the potions and denounces the player as above. E saves PC
		by dashing the potion from their hand. E gains a grudge from K. A gives K the heart and gains favour.
	3bi. E gets the wolfsheart before PC and makes the potion. PC sees L switch the potion, and so has the chance to save E before she drinks it.
		i) PC saves E, they will both gain a grudge from K and a favour from E. PC gains grudge from L. A gives the king the heart and gains favour.
		ii) If they don't, events happen as per 1a. 
	
	Intents:
	Aedelfrith - hunt the wolf
	Eoforhild - gather the wolfsheart
	Leofdaeg - kill Eoforhild, and blame it on the PC 
	
	Favours and Grudges
	Beginning
			CHARACTER
			P	A	E	L
	F	K	N	N	N	N
	A	A	F	-	F	G
	V	E	N	F	-	G
		L	N	G	G	-
		
	1ai. 	P	A	E	L	ii.		P	A	E	L
		K	N	N	-	F		K	N	N	-	F
		A	N	-	-	F		A	F	-	-	G
		E	-	-	-	-		E	-	-	-	-
		L	N	N	-	-		L	N	N	-	-
		
	1b.		P	A	E	L
		K	N	N	-	F
		A	N	-	-	G
		E	-	-	-	-
		L	N	N	-	-
		
	2ai.	P	A	E	L	ii.		P	A	E	L
		K	G	F	-	N		K	F	N	-	N			
		A	G	-	-	F		A	G	-	-	N			
		E	-	-	-	-		E	-	-	-	-
		L	N	-	-	-		L	G	G	-	-
	
	3ai.	P	A	E	L
		K	N	F	G	N					
		A	N	-	N	N				
		E	F	N	-	N
		L	G	N	G	-
	
	Summer: task - 
	
	Autumn: task -
	
	Winter: task -
	*/
}
