




    var teclado: AnimationClip;
    
    function Update () {
    
    if (Input.GetKey("1"))
    		{
    		animation.Play ("anim_attack_01");}
    		
    else if(Input.GetKey("2"))
    		{
  			  animation.Play ("anim_attack_02");}
  			  
  	else if(Input.GetKey("3"))
    		{
  			  animation.Play ("anim_attack_03");}		  

    else if(Input.GetKey("4"))
    		{
  			  animation.Play ("anim_hit");}
  	
  	else if(Input.GetKey("5"))
    		{
  			  animation.Play ("anim_lose");}
		  
  	else if(Input.GetKey("6"))
    		{
  			  animation.Play ("anim_run");}		  

    else if(Input.GetKey("7"))
    		{
  			  animation.Play ("anim_run_jump");}
  	
  	else if(Input.GetKey("8"))
    		{
  			  animation.Play ("anim_walk");}  	

    else if(Input.GetKey("9"))
    		{
  			  animation.Play ("anim_death_1");}
  			  
  	else if(Input.GetKey("q"))
    		{
  			  animation.Play ("anim_death_2");}		  

    else if(Input.GetKey("w"))
    		{
  			  animation.Play ("anim_jump");}
  	
  	else if(Input.GetKey("e"))
    		{
  			  animation.Play ("anim_win");}
  			  
  	else if(Input.GetKey("r"))
    		{
  			  animation.Play ("anim_get_up");}		
  			  	  
    else if(Input.GetKey("t"))
    		{
  			  animation.Play ("anim_boring");}  			  
  			  
  	else 
   			 {
    		animation.CrossFadeQueued ("anim_idle");}

   	 }
    



    
    
    
    
    
