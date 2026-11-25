import { Link } from "react-router-dom";
import {
  BicepsFlexed,
  Brain,
  HeartHandshake,
  PoundSterling,
  Repeat2,
  UsersRound,
  Utensils,
} from "lucide-react";
import SectionHeading from "../../components/SectionHeading";

const pillars = [
  {
    name: "Physical",
    description:
      "Move, recover and build a foundation that supports your life.",
    icon: BicepsFlexed,
  },
  {
    name: "Dietary",
    description:
      "Develop a more intentional relationship with how you fuel yourself.",
    icon: Utensils,
  },
  {
    name: "Social",
    description: "Strengthen the relationships and connections that matter.",
    icon: UsersRound,
  },
  {
    name: "Emotional",
    description: "Make space to understand and respond to your emotions.",
    icon: HeartHandshake,
  },
  {
    name: "Intellectual",
    description: "Stay curious, challenge yourself and keep learning.",
    icon: Brain,
  },
  {
    name: "Habitual",
    description:
      "Build practical routines that make positive change sustainable.",
    icon: Repeat2,
  },
  {
    name: "Financial",
    description:
      "Explore the habits and decisions that support financial wellbeing.",
    icon: PoundSterling,
  },
];

function Home() {
  return (
    <>
      <section className="hero">
        <div className="hero-glow" aria-hidden="true" />
        <div className="container hero-grid">
          <div className="hero-copy">
            <span className="eyebrow">
              <span aria-hidden="true" />A better way forward
            </span>
            <h1>
              Asistym
              <br />
              <em>for you.</em>
            </h1>
            <p>
              Life doesn’t come with a blueprint. Asistym helps you build healthier habits and move forward with purpose.
            </p>
            <div className="hero-actions">
              <Link className="button" to="/shop">
                Discover the book
              </Link>
              <Link className="text-link" to="/services">
                Explore the approach
              </Link>
            </div>
            <div className="hero-caption">
              <span className="caption-line" /> For men ready to make meaningful
              change.
            </div>
          </div>
          <div
            className="hero-visual"
            aria-label="The seven pillars of wellbeing"
          >
            <div className="visual-top">
              <span>THE ASISTYM APPROACH</span>
              <span>01 — 07</span>
            </div>
            <div className="orbit orbit-one" />
            <div className="orbit orbit-two" />
            <div className="orbit orbit-three" />
            <div className="visual-core">
              <span>YOUR</span>
              <strong>
                whole
                <br />
                wellbeing<span>.</span>
              </strong>
              <small>ONE STEP AT A TIME</small>
            </div>
            <div className="visual-tag tag-one">01 / PHYSICAL</div>
            <div className="visual-tag tag-two">04 / EMOTIONAL</div>
            <div className="visual-tag tag-three">07 / FINANCIAL</div>
            <div className="visual-bottom">
              <span>GROW WITH INTENTION</span>
              <span>●</span>
            </div>
          </div>
        </div>
        <div className="container hero-bottom">
          <span>PERSONAL DEVELOPMENT, MADE PRACTICAL</span>
          <a href="#pillars">
            Discover the pillars
          </a>
        </div>
      </section>

      <section className="intro-section section">
        <div className="container intro-grid">
          <div>
            <span className="eyebrow">
              <span aria-hidden="true" />
              The bigger picture
            </span>
            <h2>
              Wellbeing is more
              <br />
              than <em>one thing.</em>
            </h2>
          </div>
          <div className="intro-copy">
            <p>
              When one area of life feels out of balance, it can affect
              everything else. Real progress starts by looking at the whole
              picture.
            </p>
            <p>
              Asistym offers a supportive, structured space to understand
              where you are, decide what matters and take realistic steps
              forward.
            </p>
            <Link className="text-link" to="/about">
              Get to know Asistym
            </Link>
          </div>
        </div>
      </section>

      <section className="book-section" aria-labelledby="book-heading">
        <div className="book-copy">
          <span className="eyebrow">
            <span aria-hidden="true" />
            A new chapter
          </span>
          <h2 id="book-heading">
            A little perspective.<br />
            <em>A lot of potential.</em>
          </h2>
          <p>
            The Asistym book is in the works. Created to support your
            personal development, it will be a place to pause, reflect and
            explore what moving forward can look like for you.
          </p>
          <p className="book-note">Book details and availability coming soon.</p>
          <Link className="button book-button" to="/shop">
            Explore the shop
          </Link>
        </div>
        <div className="book-art" aria-hidden="true">
          <div className="book-orbit book-orbit-one" />
          <div className="book-orbit book-orbit-two" />
          <div className="book-object">
            <span>ASISTYM</span>
            <div className="book-symbol"><i /><i /><i /></div>
            <small>A NEW CHAPTER<br />COMING SOON</small>
          </div>
          <div className="book-art-caption">A RESOURCE FOR THE JOURNEY</div>
        </div>
      </section>

      <section className="pillars-section section" id="pillars">
        <div className="container">
          <div className="section-heading-row">
            <SectionHeading
              eyebrow="A whole-person approach"
              title="Seven pillars. One you."
              description="Each part of your life is connected. Together, we’ll explore the areas that matter to you."
            />
            <span className="section-index">THE ASISTYM FRAMEWORK / 01</span>
          </div>
          <div className="pillar-grid">
            {pillars.map((pillar, index) => {
              const Icon = pillar.icon;

              return (
                <article className="pillar-card" key={pillar.name}>
                  <div className="pillar-card-top">
                    <span className="pillar-icon" aria-hidden="true">
                      <Icon size={28} strokeWidth={1.75} />
                    </span>
                    <span>0{index + 1}</span>
                  </div>
                  <h3>{pillar.name}</h3>
                  <p>{pillar.description}</p>
                </article>
              );
            })}
            <div className="pillar-card pillar-final">
              <span>THE STARTING POINT</span>
              <p>
                Every journey
                <br />
                is different.
              </p>
              <Link to="/contact">
                Let’s talk
              </Link>
            </div>
          </div>
        </div>
      </section>

      <section className="journey-section section">
        <div className="container journey-grid">
          <div>
            <SectionHeading
              eyebrow="Your journey"
              title="A little structure. A lot of possibility."
              description="No quick fixes or one-size-fits-all plans. Just thoughtful support and practical steps that fit your life."
            />
            <Link className="button button-outline" to="/services">
              Explore services
            </Link>
          </div>
          <div className="journey-steps">
            <article>
              <span>01</span>
              <div>
                <h3>Take stock</h3>
                <p>
                  Start with a conversation about where you are and what you’d
                  like to change.
                </p>
              </div>
            </article>
            <article>
              <span>02</span>
              <div>
                <h3>Make a plan</h3>
                <p>
                  Identify meaningful priorities and work towards realistic,
                  personal goals.
                </p>
              </div>
            </article>
            <article>
              <span>03</span>
              <div>
                <h3>Build your momentum</h3>
                <p>
                  Reflect, adapt and develop habits you can carry forward
                  independently.
                </p>
              </div>
            </article>
          </div>
        </div>
      </section>

      <section className="statement-section">
        <div className="container statement-inner">
          <span className="eyebrow">
            <span aria-hidden="true" />
            Progress with purpose
          </span>
          <p>
            It’s not about becoming someone else.
            <br />
            <em>It’s about becoming more yourself.</em>
          </p>
          <Link className="button" to="/contact">
            Start a conversation
          </Link>
        </div>
      </section>

      <section className="discover-section section">
        <div className="container">
          <div className="section-heading-row">
            <SectionHeading
              eyebrow="Keep exploring"
              title="More for your journey."
            />
            <Link className="text-link" to="/blog">
              Visit the blog
            </Link>
          </div>
          <div className="discover-grid">
            <Link className="discover-card discover-journal" to="/blog">
              <span>THE BLOG</span>
              <div>
                <h3>
                  Ideas for a more
                  <br />
                  intentional life.
                </h3>
                <p>Articles and perspectives, coming soon.</p>
                <span className="discover-link">Explore the blog</span>
              </div>
            </Link>
            <Link className="discover-card discover-shop" to="/shop">
              <span>THE ASISTYM SHOP</span>
              <div>
                <h3>
                  Tools for
                  <br />
                  moving forward.
                </h3>
                <p>Books and resources, coming soon.</p>
                <span className="discover-link">Explore the shop</span>
              </div>
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}


export default Home;
