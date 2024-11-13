document.addEventListener('DOMContentLoaded', () => {
    const projectsContainer = document.getElementById('projects-container');
    const projects = [
        {
            name: 'Project 1',
            description: 'Description of project 1.',
            link: '#'
        },
        {
            name: 'Project 2',
            description: 'Description of project 2.',
            link: '#'
        }
        // Add more projects as needed
    ];

    projects.forEach(project => {
        const projectDiv = document.createElement('div');
        projectDiv.classList.add('project');
        projectDiv.innerHTML = `
            <h3>${project.name}</h3>
            <p>${project.description}</p>
            <a href="${project.link}">Read More</a>
        `;
        projectsContainer.appendChild(projectDiv);
    });
});
