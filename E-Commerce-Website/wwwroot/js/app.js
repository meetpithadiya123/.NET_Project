
document.addEventListener("DOMContentLoaded",()=>{
  document.querySelectorAll("[data-year]").forEach(e=>e.textContent=new Date().getFullYear());
  document.querySelectorAll(".delete-btn").forEach(btn=>btn.addEventListener("click",()=>{
    if(confirm("Delete this item?")) btn.closest("tr")?.remove();
  }));
  const search=document.querySelector("[data-search]");
  if(search){
    search.addEventListener("input",()=>{
      const q=search.value.toLowerCase();
      document.querySelectorAll("[data-searchable]").forEach(row=>{
        row.style.display=row.textContent.toLowerCase().includes(q)?"":"none";
      });
    });
  }
});
