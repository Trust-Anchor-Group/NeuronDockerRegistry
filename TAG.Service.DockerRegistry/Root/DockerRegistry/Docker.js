async function DockerAreYouSure(e, message)
{
    e.preventDefault()
    const proceed = await Popup.Confirm(message)
    if (proceed)
        e.target.submit()
}