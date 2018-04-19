
# Release notes for $defname
**Release Number**  : $($release.name)    
**Release completed** $("{0:dd/MM/yy HH:mm:ss}" -f [datetime]$release.modifiedOn) 

## Builds  
@@BUILDLOOP@@
### $($build.definition.name)  
**Build Number**  : $($build.buildnumber)    
**Build completed** $("{0:dd/MM/yy HH:mm:ss}" -f [datetime]$build.finishTime)     
**Source Branch** $($build.sourceBranch)  

![Build Badge](https://respawnsive.visualstudio.com/_apis/public/build/definitions/f1fa8967-2f73-4847-8f20-859d9d6241ca/7/badge)
  
### Associated work items  
@@WILOOP@@  
* **$($widetail.fields.'System.WorkItemType') $($widetail.id)** [Assigned by: $($widetail.fields.'System.AssignedTo')] $($widetail.fields.'System.Title')  
@@WILOOP@@  
  
### Associated change sets/commits  
@@CSLOOP@@  
* **ID $($csdetail.changesetid)$($csdetail.commitid)$($csdetail.id)** $($csdetail.comment)    
@@CSLOOP@@  


----------

@@BUILDLOOP@@